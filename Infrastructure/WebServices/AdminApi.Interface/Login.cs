using System;
using System.Collections.Generic;

namespace AFT.RegoV2.AdminApi.Interface
{
    public class LoginRequest 
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class LoginResponse 
    {
        public string Token { get; set; }
    }
}
