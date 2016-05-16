using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class WithdrawalCountException : Exception
    {
        public WithdrawalCountException() : base("Withdrawal count doesn't valid for current operation.")
        {
            
        }
    }
}
