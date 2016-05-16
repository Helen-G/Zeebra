using System;
using FluentValidation;

namespace AFT.RegoV2.Core.Common.Extensions
{
    public static class FluentValidationExtensions
    {
        public static IRuleBuilderOptions<T, TProperty> WithMessage<T, TProperty>(this IRuleBuilderOptions<T, TProperty> rule, Enum error)
        {
            var errorName = Enum.GetName(error.GetType(), error);
            return rule.WithMessage(errorName, (object[])null);
        }
    }
}