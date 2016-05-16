using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class TotalDepositAmountException : Exception
    {
        public TotalDepositAmountException()
            : base("Player's total deposit amount doesn't match configured criteria")
        {
            
        }
    }
}
