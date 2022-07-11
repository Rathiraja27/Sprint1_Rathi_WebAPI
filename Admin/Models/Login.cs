using System;
using System.Collections.Generic;

#nullable disable

namespace Admin.Models
{
    public partial class Login
    {
        public Login()
        {
            RefreshTokens = new HashSet<RefreshToken>();
        }

        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Mode { get; set; }

        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
    }
}
