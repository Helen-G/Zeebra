using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using FluentValidation;

namespace AFT.RegoV2.AdminWebsite.Validators
{
    public class ProductSettingsValidator : AbstractValidator<BrandProductSettingsData>
    {
        public ProductSettingsValidator(IGameRepository repository)
        {
        }
    }
}