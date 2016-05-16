using System;
using System.Collections.Generic;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class LoginRequest 
    {
        public Guid BrandId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string IPAddress { get; set; }
        public Dictionary<string, string> RequestHeaders { get; set; }
        
    }

    public class LoginResponse 
    {
        public string Token { get; set; }
    }
}
