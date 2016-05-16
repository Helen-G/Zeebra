using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.ApplicationServices;
using AFT.RegoV2.Core.Bonus.Resources;
using FluentValidation;

namespace AFT.RegoV2.Core.Bonus.DomainServices
{
    internal class TemplateDeletionValidator : AbstractValidator<Guid>
    {
        public TemplateDeletionValidator(BonusQueries queries)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(templateId => templateId)
                .Must(templateId => queries.GetCurrentVersionTemplates().Any(t => t.Id == templateId))
                .WithMessage(ValidatorMessages.TemplateDoesNotExist)
                .Must(templateId =>
                        queries.GetCurrentVersionBonuses().Any(bonus => bonus.Template.Id == templateId) == false)
                .WithMessage(ValidatorMessages.TemplateIsInUse)
                .WithName("Template");
        }
    }
}