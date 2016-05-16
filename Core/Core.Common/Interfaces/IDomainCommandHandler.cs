namespace AFT.RegoV2.Core.Common.Interfaces
{
    public interface IDomainCommandHandler<in TCommand> where TCommand : IDomainCommand
    {
        void Handle(TCommand command);
    }

    public interface IDomainCommandHandler<in TCommand, TResult>
        where TCommand : IDomainCommand
    {
        TResult Handle(TCommand command);
    }
}