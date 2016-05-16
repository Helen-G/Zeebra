using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Common.Data;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Player.Data
{
    public class PlayerCommandResult
    {
        public bool Success { get; set; }
        public Player Player { get; set; }
        public ValidationResult ValidationResult { get; set; }
    }

    public class LoginRequestContext
    {
        public Dictionary<string,string> BrowserHeaders { get; set; }
        public string IpAddress { get; set; }
        public Guid BrandId { get; set; }
    }

   
}
