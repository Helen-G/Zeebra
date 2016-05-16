using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Brand.Data;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Events.Brand;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class EditVipLevelValidator : AbstractValidator<VipLevelViewModel>
    {
        public EditVipLevelValidator(IBrandRepository brandRepository, IEnumerable<GameDTO> games)
        {
            const int min = 1;
            const int maxCode = 20;
            const int maxName = 50;
            const int maxDescription = 200;
            const int maxRankLength = 1000;

            CascadeMode = CascadeMode.Continue;

            RuleFor(x => x.Brand)
                .Must(x => brandRepository.Brands.Any(y => y.Id == x))
                    .WithMessage("{\"text\": \"app:vipLevel.noBrand\"}");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Matches(@"^[a-zA-Z-0-9]+$")
                    .WithMessage("{\"text\": \"app:vipLevel.codeCharError\"}")
                .Length(min, maxCode)
                    .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", maxCode);

            RuleFor(x => x)
                .Must(x => !brandRepository.VipLevels.Any(y => y.Code == x.Code && y.Id != x.Id))
                    .WithMessage("{\"text\": \"app:common.codeUnique\"}")
                .WithName("Code");

            RuleFor(x => x.Name)
                .NotEmpty()
                    .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Matches(@"^[a-zA-Z0-9-_ ]+$").WithMessage("{\"text\": \"app:vipLevel.nameCharError\"}")
                .Length(min, maxName)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", maxName);

            RuleFor(x => x)
                .Must(x => !brandRepository.VipLevels.Any(y => y.Name == x.Name && y.Id != x.Id))
                .WithName("Name")
                .WithMessage("{\"text\": \"app:common.nameUnique\"}")
                .Must(x =>
                {
                    var vipLevel = brandRepository.VipLevels.SingleOrDefault(y => y.Id == x.Id);
                    return vipLevel != null && vipLevel.Status == VipLevelStatus.Inactive;
                })
                .WithName("Name")
                .WithMessage("{\"text\": \"app:vipLevel.editActiveRecordError\"}");

            RuleFor(x => x.Rank)
                .GreaterThanOrEqualTo(min)
                    .WithMessage("{\"text\": \"app:common.validationMessages.onlyNumbersBetweenOneAndThousand\"}")
                .LessThanOrEqualTo(maxRankLength)
                    .WithMessage("{\"text\": \"app:common.validationMessages.onlyNumbersBetweenOneAndThousand\"}");

            RuleFor(x => x)
                .Must(x => !brandRepository.VipLevels.Any(y => y.Rank == x.Rank && y.Brand.Id == x.Brand && y.Id != x.Id))
                    .WithMessage("{\"text\": \"app:vipLevel.rankUniqueError\"}")
                .WithName("Rank");

            When(x => !string.IsNullOrWhiteSpace(x.Description), () => RuleFor(x => x.Description)
                .Length(min, maxDescription)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", maxDescription));

            When(x => !string.IsNullOrWhiteSpace(x.Color), () => RuleFor(x => x.Color)
                .Matches(@"^#[0-9a-fA-F]{6}").WithMessage("{\"text\": \"app:vipLevel.colorCharError\"}"));

            When(x => x.Limits.Any(), () => RuleFor(x => x.Limits)
                .Must(x => x.All(y => y.GameProviderId.HasValue))
                    .WithName("GameValidation")
                    .WithMessage("{\"text\": \"app:vipLevel.gameRequired\"}")
                .Must(x => x.All(y => !string.IsNullOrWhiteSpace(y.CurrencyCode)))
                    .WithName("GameValidation")
                    .WithMessage("{\"text\": \"app:vipLevel.currencyRequired\"}")
                .Must(x => x.All(y => y.BetLimitId.HasValue))
                    .WithName("GameValidation")
                    .WithMessage("{\"text\": \"app:vipLevel.selectBetLevel\"}")
                .Must(y => !y.GroupBy(i => new { i.CurrencyCode, GameProviderId = i.GameProviderId }).Any(j => j.Count() > 1))
                    .WithName("GameValidation")
                    .WithMessage("{\"text\": \"app:vipLevel.duplicatedCurrencyAndGame\"}"));
        }
    }
}