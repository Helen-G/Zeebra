using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace AFT.RegoV2.Core.Fraud.Validations
{
    public class UpdateRiskLevelValidator : RiskLevelValidatorBase
    {
        public UpdateRiskLevelValidator(IFraudRepository repository)
            : base(repository)
        {
            RuleFor(x => x.Id)
                .Must(id => repository.RiskLevels.Any(y => id == y.Id)).WithMessage("{\"text\": \"app:common.idDoesNotExist\"}");
        }
    }
}
