using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class FraudlentRiskException : Exception
    {
        public FraudlentRiskException() : base("User has been tagged with fraudlent risk level.")
        {
        }
    }
}
