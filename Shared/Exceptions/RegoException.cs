using System;

namespace AFT.RegoV2.Shared
{
    /// <summary>
    /// Generic Rego exception. All specific Rego exceptions should be derived from this type
    /// </summary>
    public class RegoException : Exception
    {
        public RegoException(string message) : base(message) { }
        public RegoException(string message, Exception inner) : base(message, inner) {}
    }
}
