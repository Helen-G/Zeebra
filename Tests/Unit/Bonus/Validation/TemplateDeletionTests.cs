using System;
using System.Linq;
using AFT.RegoV2.Core.Bonus.Resources;
using AFT.RegoV2.Tests.Common.Base;
using FluentAssertions;
using NUnit.Framework;

namespace AFT.RegoV2.Tests.Unit.Bonus.Validation
{
    class TemplateDeletionTests : BonusTestsBase
    {
        [Test]
        public void Can_not_delete_non_existent_template()
        {
            var result = BonusQueries.GetValidationResult(Guid.Empty);

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.TemplateDoesNotExist);
        }

        [Test]
        public void Can_not_delete_template_if_there_are_bonuses_using_it()
        {
            var bonus = BonusHelper.CreateBasicBonus();
            BonusRepository.Templates.Add(bonus.Template);
            var result = BonusQueries.GetValidationResult(bonus.Template.Id);

            result.Errors.Single().ErrorMessage.Should().Be(ValidatorMessages.TemplateIsInUse);
        }
    }
}