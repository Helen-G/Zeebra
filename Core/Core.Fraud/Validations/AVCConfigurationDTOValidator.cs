using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Fraud.Data;
using FluentValidation;
using FluentValidation.Results;

namespace AFT.RegoV2.Core.Fraud.Validations
{
    public class AVCConfigurationDTOValidator: AbstractValidator<AVCConfigurationDTO>
    {
        public AVCConfigurationDTOValidator(IFraudRepository fraudRepository, AvcConfigurationDtoQueriesEnum queryType)
        {
            When(formData => formData.HasPaymentLevel, () =>
            {
                RuleFor(formData => formData.PaymentLevels)
                    .Cascade(CascadeMode.StopOnFirstFailure)
                    .Must(numberOfPaymentLevel => numberOfPaymentLevel.Any())
                    .WithMessage(AVCConfigurationValidationMessagesEnum.AtLeastOnePaymentLevelIsNeeded.ToString());
            });

            switch (queryType)
            {
                case AvcConfigurationDtoQueriesEnum.Create:
                    Custom(config =>
                    {
                        return fraudRepository.AutoVerificationCheckConfigurations.Any(
                            record =>
                                record.BrandId == config.Brand &&
                                record.Currency == config.Currency &&
                                record.VipLevelId == config.VipLevel) ? new ValidationFailure("", AVCConfigurationValidationMessagesEnum.RecordWithTheSameCompositeKeyAlreadyExists.ToString()) : null;
                    }); break;

                case AvcConfigurationDtoQueriesEnum.Update:
                    Custom(config =>
                    {
                        return fraudRepository.AutoVerificationCheckConfigurations
                            .Where(record => record.Id != config.Id)
                            .Any(
                            record =>
                                record.BrandId == config.Brand &&
                                record.Currency == config.Currency &&
                                record.VipLevelId == config.VipLevel) ? new ValidationFailure("", AVCConfigurationValidationMessagesEnum.RecordWithTheSameCompositeKeyAlreadyExists.ToString()) : null;
                    });
                    break;
            }         
        }
    }

    public enum AvcConfigurationDtoQueriesEnum
    {
        Create,
        Update
    }
}
