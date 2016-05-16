using System.Transactions;
using AFT.RegoV2.Core.Common.Utils;

namespace AFT.RegoV2.Infrastructure.Providers
{
    public interface ITransactionScopeProvider
    {
        TransactionScope GetTransactionScope();
        TransactionScope GetTransactionScopeAsync();
    }
    public sealed class TransactionScopeProvider : ITransactionScopeProvider
    {
        TransactionScope ITransactionScopeProvider.GetTransactionScope()
        {
            return CustomTransactionScope.GetTransactionScope();
        }
        TransactionScope ITransactionScopeProvider.GetTransactionScopeAsync()
        {
            return CustomTransactionScope.GetTransactionScopeAsync();
        }
    }
}