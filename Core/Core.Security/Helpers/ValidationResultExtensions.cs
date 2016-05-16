using System.Linq;
using ServiceStack.FluentValidation.Results;
using ServiceStack.Validation;

namespace AFT.RegoV2.Domain.BoundedContexts.Security.Helpers
{
    public static class ValidationResultExtensions
    {
        public static ValidationError GetValidationError(this ValidationResult validationResult)
        {
            var validationErrorFields = validationResult.Errors.Select(x => new ValidationErrorField(x.ErrorCode, x.PropertyName, x.ErrorMessage)).ToArray();
            var firstError = validationResult.Errors.First();
            return new ValidationError(new ValidationErrorResult(validationErrorFields) { ErrorMessage = firstError.ErrorMessage, ErrorCode = firstError.ErrorCode });
        }
    }
}