using System;
using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Domain.Player;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Domain.Player.Resources;
using ServiceStack.FluentValidation;

namespace AFT.RegoV2.Core.Player.Validators
{
    public class ChangeSecurityQuestionValidator : AbstractValidator<ChangeSecurityQuestionData>
    {

        public ChangeSecurityQuestionValidator(IPlayerRepository repository)
        {
            CascadeMode = CascadeMode.StopOnFirstFailure;

            RuleFor(p => p.Id)
                .NotEmpty()
                .WithMessage(PlayerAccountResponseCode.PlayerDoesNotExist.ToString());

            //invalid player guid
            RuleFor(p => p.Id)
                .Must((change, playerId) =>
                {
                    Guid tmpPlayerGuid;
                    return Guid.TryParse(playerId, out tmpPlayerGuid);
                })
                .WithMessage(PlayerAccountResponseCode.PlayerDoesNotExist.ToString());

            RuleFor(p => p.Id)
                .Must((change, playerId) =>
                {
                    Guid id;
                    Guid.TryParse(playerId, out id);

                    return repository.Players.Count(player => player.Id == id) > 0;
                })
                .WithMessage(PlayerAccountResponseCode.PlayerDoesNotExist.ToString());

            RuleFor(p => p.SecurityQuestionId)
                .NotEmpty()
                .WithMessage(RegisterValidatorResponseCodes.SecurityQuestionIdRequired.ToString());

            RuleFor(p => p.SecurityAnswer)
                .NotEmpty()
                .WithMessage(RegisterValidatorResponseCodes.SecurityAnswerIsMissing.ToString());

            Guid tmpGuid;

            When(p => !string.IsNullOrWhiteSpace(p.SecurityQuestionId) && Guid.TryParse(p.SecurityQuestionId, out tmpGuid),
                () =>
                {
                    RuleFor(req => req.SecurityQuestionId)
                        .Must((r, qId) =>
                        {
                            Guid questionId = Guid.Parse(qId);
                            return repository.SecurityQuestions.Count(q => q.Id == questionId) > 0;
                        })
                        .WithMessage(RegisterValidatorResponseCodes.InvalidSecurityQuestionId.ToString());
                });
        }

    }
}