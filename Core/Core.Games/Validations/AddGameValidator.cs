using System.Linq;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Interfaces;
using ServiceStack.FluentValidation;

namespace AFT.RegoV2.Core.Game.Validations
{
    public class AddGameValidator : AbstractValidator<GameDTO>
    {
        public AddGameValidator(
            IGameRepository gameRepository)
        {
            CascadeMode = CascadeMode.Continue;

            RuleFor(x => x.ProductId)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");
            RuleFor(x => x.Name)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");
            RuleFor(x => x.Url)
                .NotNull()
                .WithMessage("{\"text\": \"app:common.requiredField\"}");
            RuleFor(x => x)
                .Must(x => !gameRepository.GameProviderConfigurations.Any(y => y.Name == x.Name && x.ProductId == y.GameProviderId))
                .WithName("name")
                .WithMessage("{\"text\": \"app:gameIntegration.games.nameUnique\"}");
        }
    }
}