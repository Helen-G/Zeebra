using System;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.ApplicationServices.Security
{
    public class SecurityException : RegoException
    {
        public SecurityException(string message)
            : base(message)
        {
        }
    }

    public class InsufficientPermissionsException : SecurityException
    {
        public InsufficientPermissionsException(string message)
            : base(message)
        {
        }
    }
}
