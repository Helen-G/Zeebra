using System;

namespace AFT.RegoV2.Core.Fraud.Exceptions
{
    public class AccountAgeException : Exception
    {
        #region Constructors

        public AccountAgeException() : base("Account age doesn't allow this operation.")
        {
        }

        #endregion
    }
}