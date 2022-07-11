using Admin.Interfaces;
using Admin.Models;
using Admin.ViewModel;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// JWT Token Authentiation Repository
/// </summary>
namespace Admin.Service
{
    public class JWTManagerRepository : IJWTManagerRepository
    {
        #region Variable Declaration

        LoginViewModel userRecords = new LoginViewModel();
        private readonly IConfiguration configuration;
        private readonly FlightDBContext _db;
        public Dictionary<string,string> RefreshedTokenUser = new Dictionary<string, string>();

        #endregion

        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="JWTManagerRepository" /> class.</summary>
        /// <param name="_configuration">The configuration.</param>
        /// <param name="db">The database.</param>
        public JWTManagerRepository(IConfiguration _configuration, FlightDBContext db)
        {
            configuration = _configuration;
            _db = db;
        }

        #endregion

        #region Public Methods

        /// <summary>Authenticates the specified users.</summary>
        /// <param name="users">The Logged user details</param>
        /// <param name="isRegister">True if it is registration mode else false</param>
        /// <returns>The Generated token</returns>
        public Tokens Authenticate(LoginViewModel users, bool isRegister)
        {
            if (isRegister)
            {
                if (_db.Logins.Any(x => x.UserName == users.UserName))
                {
                    return null;
                }

                Login tbllogin = new Login();
                tbllogin.UserName = users.UserName;
                tbllogin.Password = users.Password;
                _db.Logins.Add(tbllogin);
                _db.SaveChanges();
            }

            if (!_db.Logins.Any(x => x.UserName == users.UserName && x.Password == users.Password))
            {
                return null;
            }
            else
            {
                var user= _db.Logins.Where(x => x.UserName == users.UserName && x.Password == users.Password).FirstOrDefault();
                userRecords.UserName = user.UserName;
                userRecords.Password = user.Password;
                userRecords.Role = user.Mode;
                userRecords.Id = user.Id;
            }
          
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenkey = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.Name, userRecords.UserName),
                new Claim(ClaimTypes.Role, userRecords.Role)
                }),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = GenerateToken();

            if (RefreshedTokenUser.ContainsKey(userRecords.UserName))
            {
                RefreshedTokenUser[userRecords.UserName] = refreshToken;
            }


            if (!_db.RefreshTokens.Any(x => x.UserId == userRecords.Id))
            {
                Models.RefreshToken tokenDetails = new Models.RefreshToken();
                tokenDetails.UserId = userRecords.Id;
                tokenDetails.Token = token.ToString();
                tokenDetails.RefreshedToken = refreshToken;
                _db.RefreshTokens.Add(tokenDetails);
                _db.SaveChanges();
            }
            else
            {
                var data = _db.RefreshTokens.Where(x => x.UserId == userRecords.Id).FirstOrDefault();
                data.Token = token.ToString();
                data.RefreshedToken = refreshToken;
                _db.RefreshTokens.Update(data);
                _db.SaveChanges();
            }

            return new Tokens
            {
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken
            };
            
        }

        /// <summary>Refreshes the specified credentials.</summary>
        /// <param name="refresh"></param>
        /// <returns>Returns the Refreshed token</returns>
        /// <exception cref="SecurityTokenException">Invalid token Passed</exception>
        public Tokens Refresh(RefreshCredentials refresh)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
            SecurityToken securityToken;
            var validate = tokenHandler.ValidateToken(refresh.Token,
                new TokenValidationParameters
                {
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                }, out securityToken);

            var jwtToken = securityToken as JwtSecurityToken;

            if(jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token Passed");
            }

            var userName = validate.Identity.Name;
            var user = _db.Logins.Where(x => x.UserName == userName).FirstOrDefault();

            if (refresh.RefreshToken != _db.RefreshTokens.Where(x=> x.UserId == user.Id).Select(x=> x.RefreshedToken).FirstOrDefault())
            {
                throw new SecurityTokenException("Invalid token Passed");
            }

            return Authenticate(userName, validate.Claims.ToArray());
        }


        /// <summary>Authenticates the specified user name.</summary>
        /// <param name="userName">Name of the user.</param>
        /// <param name="claims">The claims.</param>
        /// <returns>Returns the Refreshed Token </returns>
        public Tokens Authenticate(string userName, Claim[] claims)
        {
            var key = Encoding.UTF8.GetBytes(configuration["JWT:Key"]);
            var securityToken = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
                );

            var token = new JwtSecurityTokenHandler().WriteToken(securityToken);

            var refreshToken = GenerateToken();

            if (RefreshedTokenUser.ContainsKey(userName))
            {
                RefreshedTokenUser[userName] = refreshToken;
            }
            else 
            {
                RefreshedTokenUser.Add(userName, refreshToken);
            }

            return new Tokens
            {
                Token = token,
                RefreshToken = refreshToken
            };

        }

        #endregion

        #region Private Methods

        /// <summary>Generates the token.</summary>
        /// <returns>Generated Token</returns>
        private string GenerateToken()
        {
            var random = new byte[32];
            using(var randomNumber = RandomNumberGenerator.Create())
            {
                randomNumber.GetBytes(random);
                return Convert.ToBase64String(random);
            }
        }

        #endregion
    }
}
