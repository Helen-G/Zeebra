using System;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Common.Extensions;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Data.Commands;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;
using FluentValidation;

namespace AFT.RegoV2.Core.Payment.Validators
{
    public class DeactivatePaymentLevelValidator : AbstractValidator<DeactivatePaymentLevelCommand>
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly PaymentLevelQueries _paymentLevelQueries;
        private PaymentLevel _oldPaymentLevel;
        private PaymentLevel _newPaymentLevel;
        private bool _newPaymentLevelRequired;

        public DeactivatePaymentLevelValidator(IPaymentRepository paymentRepository, PaymentLevelQueries paymentLevelQueries)
        {
            _paymentRepository = paymentRepository;
            _paymentLevelQueries = paymentLevelQueries;

            CascadeMode = CascadeMode.StopOnFirstFailure;

            ValidatePaymentLevel();
            ValidateNewPaymentLevel();
        }

        private void ValidatePaymentLevel()
        {
            RuleFor(x => x.Id)
                .Must(x => x != Guid.Empty)
                .WithMessage(DeactivatePaymentLevelErrors.Requred)
                .Must(x =>
                {
                    _oldPaymentLevel = _paymentRepository.PaymentLevels.SingleOrDefault(y => y.Id == x);

                    return _oldPaymentLevel != null;
                })
                .WithMessage(DeactivatePaymentLevelErrors.NotFound)
                .Must(x => _oldPaymentLevel.Status == PaymentLevelStatus.Active)
                .WithMessage(DeactivatePaymentLevelErrors.NotActive)
                .Must((data, x) =>
                {
                    var deactivatePaymentLevelStatus = _paymentLevelQueries.GetDeactivatePaymentLevelStatus(x);

                    _newPaymentLevelRequired =
                        deactivatePaymentLevelStatus == DeactivatePaymentLevelStatus.CanDeactivateIsAssigned ||
                        deactivatePaymentLevelStatus == DeactivatePaymentLevelStatus.CanDeactivateIsDefault;
                        
                    return !_newPaymentLevelRequired ||
                           (data.NewPaymentLevelId.HasValue && data.NewPaymentLevelId.Value != Guid.Empty);
                })
                .WithMessage(DeactivatePaymentLevelErrors.NewPaymentLevelRequired);
        }

        private void ValidateNewPaymentLevel()
        {
            When(x => _newPaymentLevelRequired && x.NewPaymentLevelId.HasValue, () => RuleFor(x => x.NewPaymentLevelId)
                .Must(x =>
                {
                    _newPaymentLevel = _paymentRepository.PaymentLevels.SingleOrDefault(y => y.Id == x.Value);

                    return _newPaymentLevel != null;
                })
                .WithMessage(DeactivatePaymentLevelErrors.NewPaymentLevelNotFound)
                .Must(x => _newPaymentLevel.Status == PaymentLevelStatus.Active)
                .WithMessage(DeactivatePaymentLevelErrors.NewPaymentLevelNotActive));
        }
    }
}
