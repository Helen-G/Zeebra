using System.Linq;
using FluentValidation.Results;

namespace AFT.RegoV2.Shared
{
    /// <summary>
    /// This class is obsolete, please avoid using it in your validation logic
    /// </summary>
    public class RegoValidationException : RegoException
    {
        public RegoValidationException(string message) : base(message)
        {
        }

        public RegoValidationException(ValidationResult validationResult)
            : base(validationResult.Errors.First().ErrorMessage)
        {
        }
    }
}