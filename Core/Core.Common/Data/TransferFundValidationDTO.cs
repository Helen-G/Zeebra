using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Common.Data
{
    public class TransferFundValidationDTO
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
}
