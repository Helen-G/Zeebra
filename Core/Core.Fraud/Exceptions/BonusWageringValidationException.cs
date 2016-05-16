using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class BonusWageringValidationException : Exception
    {
        public BonusWageringValidationException()
            : base("Player has incompleted bonus wagering requirements.")
        {
            
        }
    }
}
