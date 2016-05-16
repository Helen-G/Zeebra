using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data.Brand;
using FluentValidation;

namespace AFT.RegoV2.Core.Brand.Validators
{
    public class AddBrandValidator : AbstractValidator<AddBrandData>
    {
        public AddBrandValidator(IBrandRepository brandRepository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(x => x.Licensee)
                .Must(x => brandRepository.Licensees.SingleOrDefault(y => y.Id == x) != null)
                .WithMessage("{\"text\": \"app:brand.invalidLicensee\"}");

            RuleFor(x => x.Type)
                .Must(x => Enum.IsDefined(typeof(BrandType), x))
                .WithMessage("{\"text\": \"app:brand.invalidBrandType\"}");

            RuleFor(x => x.Name)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(1, 20)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 20)
                .Matches(@"^[a-zA-Z0-9-_\.]+$")
                .WithMessage("{\"text\": \"app:brand.nameCharError\"}")
                .Must((data, name) => !brandRepository.Brands.Any(brand =>
                    brand.Licensee.Id == data.Licensee &&
                    brand.Name == name))
                .WithMessage("{\"text\": \"app:brand.nameUnique\"}");

            RuleFor(x => x.Code)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(1, 20)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 20)
                .Matches(@"^[a-zA-Z0-9]+$")
                .WithMessage("{\"text\": \"app:brand.codeCharError\"}")
                .Must((data, code) => !brandRepository.Brands.Any(brand =>
                    brand.Licensee.Id == data.Licensee &&
                    brand.Code == code))
                .WithMessage("{\"text\": \"app:brand.codeUnique\"}");

            When(x => !x.EnablePlayerPrefix, () => RuleFor(x => x.PlayerPrefix)
                .Must(x => x == null)
                .WithMessage("{\"text\": \"app:brand.prefixShouldBeEmpty\"}"));

            When(x => x.EnablePlayerPrefix, () => RuleFor(x => x.PlayerPrefix)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Length(1, 3)
                .WithMessage("{{\"text\": \"app:common.exceedMaxLength\", \"variables\": {{\"length\": \"{0}\"}}}}", 3)
                .Matches(@"^[a-zA-Z0-9_\.]+$")
                .WithMessage("{\"text\": \"app:brand.playerPrefixCharError\"}")
                .Must((data, playerPrefix) => !brandRepository.Brands.Any(brand =>
                    brand.Licensee.Id == data.Licensee &&
                    brand.PlayerPrefix == playerPrefix))
                .WithMessage("{\"text\": \"app:brand.prefixUnique\"}"));

            RuleFor(x => x.PlayerActivationMethod)
                .Must(x => Enum.IsDefined(typeof (PlayerActivationMethod), x))
                .WithMessage("{\"text\": \"app:brand.invalidPlayerActivationMethod\"}");

            RuleFor(x => x.InternalAccounts)
                .GreaterThanOrEqualTo(0)
                .WithMessage("{{\"text\": \"app:common.numberOutOfRange\", \"variables\": {{\"minimum\": \"0\", \"maximum\": \"10\"}}}}")
                .LessThanOrEqualTo(10)
                .WithMessage("{{\"text\": \"app:common.numberOutOfRange\", \"variables\": {{\"minimum\": \"0\", \"maximum\": \"10\"}}}}");

            RuleFor(x => x.TimeZoneId)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}")
                .Must(x => TimeZoneInfo.GetSystemTimeZones().Any(y => y.Id == x))
                .WithMessage("{\"text\": \"app:common.invalidTimeZone\"}");
        }
    }
}
