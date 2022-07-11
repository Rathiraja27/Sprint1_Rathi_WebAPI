using Admin.Interfaces;
using Admin.ViewModel;
using Microsoft.AspNetCore.Mvc;

/// <summary>
/// Web Api to Validate the Admin user using the JWT Token Authentication
/// </summary>
namespace Admin.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/{v:apiVersion}/flight/admin")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        #region Variable Declaration

        private readonly IJWTManagerRepository _iJWTManagerRepository;

        #endregion

        #region Constructor

        /// <summary>Initializes a new instance of the <see cref="AdminController" /> class.</summary>
        /// <param name="iJWTManagerRepository">The i JWT manager repository.</param>
        public AdminController(IJWTManagerRepository iJWTManagerRepository)
        {
            _iJWTManagerRepository = iJWTManagerRepository;
        }

        #endregion

        #region Action Methods

        /// <summary>Logins the specified login view model.</summary>
        /// <param name="loginViewModel">The login view model.</param>
        /// <returns>Returns Ok when the user is Authorized, else Unauthorized</returns>
        [HttpPost, Route("Login")]
        public IActionResult Login(LoginViewModel loginViewModel)
        {
            var token = _iJWTManagerRepository.Authenticate(loginViewModel, false);

            if (token == null)
            {
                return Unauthorized();
            }

            return Ok(token);
        }


        /// <summary>Refreshes the specified credentials.</summary>
        /// <param name="credentials">The credentials.</param>
        /// <returns>Refreshed Credentials</returns>
        [HttpPost, Route("Refresh")]
        public IActionResult Refresh([FromBody]RefreshCredentials credentials)
        {
            var token = _iJWTManagerRepository.Refresh(credentials);

            if(token == null)
            {
                return Unauthorized();
            }

            return Ok(token);
        }

        #endregion
    }
}
