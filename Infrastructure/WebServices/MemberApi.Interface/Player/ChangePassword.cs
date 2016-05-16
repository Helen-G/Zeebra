using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class ChangePasswordRequest 
    {
        public string Username { get; set; }
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
    }

    public class ChangePasswordResponse
    {
    }
}
