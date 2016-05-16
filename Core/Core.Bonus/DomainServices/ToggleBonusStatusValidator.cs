using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Data;
using AFT.RegoV2.Core.Bonus.Data.ViewModels;
using AFT.RegoV2.Core.Bonus.Resources;
using FluentValidation;

namespace AFT.RegoV2.Core.Bonus.DomainServices
{
    internal class ToggleBonusStatusValidator : AbstractValidator<ToggleBonusStatusVM>
    {
        public ToggleBonusStatusValidator(BonusQueries queries)
        {
            Func<Guid, Data.Bonus> bonusGetter =
                (bonusId) => queries.GetCurrentVersionBonuses().SingleOrDefault(b => b.Id == bonusId);

            RuleFor(model => model.Id)
                .Must(id => bonusGetter(id) != null)
                .WithMessage(ValidatorMessages.BonusDoesNotExist);

            RuleFor(model => model.IsActive)
                .Must((model, destStatus) =>
                {
                    var bonusToValidate = bonusGetter(model.Id);
                    var trigger = bonusToValidate.Template.Info.BonusTrigger;
                    if (trigger == Trigger.ReferFriend || trigger == Trigger.MobilePlusEmailVerification)
                    {
                        var isAnyActiveBonusesOfTrigger = queries
                            .GetCurrentVersionBonuses()
                            .Where(bonus =>
                                bonus.Id != bonusToValidate.Id &&
                                bonus.Template.Info.Brand.Id == bonusToValidate.Template.Info.Brand.Id &&
                                bonus.IsActive)
                            .Any(bonus => bonus.Template.Info.BonusTrigger == trigger);
                        return isAnyActiveBonusesOfTrigger == false;
                    }

                    return true;
                })
                .WithMessage(ValidatorMessages.OneBonusOfATypeCanBeActive)
                .When(model =>
                {
                    var bonus = bonusGetter(model.Id);
                    if (bonus == null)
                        return false;

                    return bonus.IsActive == false && model.IsActive;
                });
        }
    }
}