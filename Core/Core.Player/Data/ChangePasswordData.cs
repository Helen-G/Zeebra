using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Player.Data
{
    public class ChangePasswordData
    {
        public string Username { get; set; }
        public string NewPassword { get; set; }
        public string OldPassword { get; set; }
    }
}
