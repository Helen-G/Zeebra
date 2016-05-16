using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace AFT.RegoV2.Core.Fraud.Validations
{
    public class RiskLevelValidatorBase : AbstractValidator<Data.RiskLevel>
    {
        public RiskLevelValidatorBase(IFraudRepository repository)
        {
            RuleFor(x => x.BrandId)
                .Must(id => repository.Brands.Any(y => id == y.Id)).WithMessage("{\"text\": \"app:fraud.manager.message.brandDoesNotExist\"}")
                .NotEmpty().WithMessage("{\"text\": \"app:common.requiredField\"}");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(1, 50).WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 50)
                .Matches(@"^[a-zA-Z0-9-_\s]*$").WithMessage("{\"text\": \"app:common.validationMessages.alphanumericSpacesDashesUnderscores\"}");


            RuleFor(x => x.Description)
                .Length(0, 200).WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 200);
        }
    }
}
