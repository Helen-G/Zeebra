using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.BoundedContexts.Brand;
using AFT.RegoV2.Core.Domain.GameServerIntegration.Data;
using AFT.RegoV2.Domain.BoundedContexts.Brand.ApplicationServices.Data;
using ServiceStack.FluentValidation;

namespace AFT.RegoV2.Domain.BoundedContexts.Brand.Validators
{
    public class AddVipLevelValidator : AbstractValidator<AddVipLevelCommand>
    {
        public AddVipLevelValidator(IBrandRepository brandRepository, IEnumerable<GameEndpoint> games)
        {
            const int min = 1;
            const int maxCode = 20;
            const int maxName = 50;
            const int maxDescription = 200;

            CascadeMode = CascadeMode.Continue;

            RuleFor(x => x.Brand)
                .Must(x => brandRepository.Brands.Any(y => y.Id == x)).WithMessage("{\"text\": \"app:vipLevel.noBrand\"}");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Matches(@"^[a-zA-Z-0-9]+$").WithMessage("{\"text\": \"app:vipLevel.codeCharError\"}")
                .Length(min, maxCode).WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", maxCode)
                .Must(x => !brandRepository.VipLevels.Any(y => y.Code == x)).WithMessage("{\"text\": \"app:common.codeUnique\"}");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Matches(@"^[a-zA-Z0-9-_ ]+$").WithMessage("{\"text\": \"app:vipLevel.nameCharError\"}")
                .Length(min, maxName).WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", maxName)
                .Must(x => !brandRepository.VipLevels.Any(y => y.Name == x)).WithMessage("{\"text\": \"app:common.nameUnique\"}");

            When(x => !string.IsNullOrWhiteSpace(x.Description), () => RuleFor(x => x.Description)
                .Length(min, maxDescription).WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", maxDescription));

            When(x => !string.IsNullOrWhiteSpace(x.Color), () => RuleFor(x => x.Color)
                .Matches(@"^#[0-9a-fA-F]{6}").WithMessage("{\"text\": \"app:vipLevel.colorCharError\"}"));

            When(x => x.Limits.Any(), () => RuleFor(x => x.Limits)
                .Must(x => x.All(y => y.GameId.HasValue)).WithName("GameValidation").WithMessage("{\"text\": \"app:vipLevel.gameRequired\"}")
                .Must(x => x.All(y => !string.IsNullOrWhiteSpace(y.CurrencyCode))).WithName("GameValidation").WithMessage("{\"text\": \"app:vipLevel.currencyRequired\"}")
                .Must(x => x.All(y => y.Minimum.HasValue && y.Maximum.HasValue)).WithName("GameValidation").WithMessage("{\"text\": \"app:vipLevel.minMaxRequired\"}")
                .Must(x => x.All(y => y.Minimum >= 0)).WithName("GameValidation").WithMessage("{\"text\": \"app:vipLevel.minNotNegative\"}")
                .Must(x => x.All(y => y.Maximum > y.Minimum || (y.Maximum == y.Minimum && y.Maximum == 0))).WithName("GameValidation").WithMessage("{\"text\": \"app:vipLevel.minBelowMax\"}")
                .Must(x => x.All(y => games.Any(z => z.Id == y.GameId))).WithName("GameValidation").WithMessage("{\"text\": \"app:vipLevel.noGame\"}"));
        }
    }
}