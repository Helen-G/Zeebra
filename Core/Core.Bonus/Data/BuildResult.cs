using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Bonus.Data
{
    public class BuildResult<T>
    {
        public BuildResult(T validatedEntity, ValidationResult result)
        {
            IsValid = result.IsValid;
            Errors = new List<ValidationError>();
            if (result.IsValid == false)
            {
                result.Errors.ToList()
                    .ForEach(error => Errors.Add(new ValidationError
                    {
                        PropertyName = error.PropertyName, 
                        ErrorMessage = error.ErrorMessage
                    }));
            }
            else
            {
                Entity = validatedEntity;
            }
        }

        public bool IsValid;
        public T Entity;
        public List<ValidationError> Errors;
    }

    public class ValidationError
    {
        public string PropertyName;
        public string ErrorMessage;
    }
}
