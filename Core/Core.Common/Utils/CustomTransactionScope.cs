using System.Transactions;

namespace AFT.RegoV2.Core.Common.Utils
{
    public class CustomTransactionScope
    {
        public static TransactionScope GetTransactionScope(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = isolationLevel,
                    Timeout = TransactionManager.MaximumTimeout
                });
        }

        public static TransactionScope GetTransactionScopeAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions
                {
                    IsolationLevel = isolationLevel,
                    Timeout = TransactionManager.MaximumTimeout
                }, TransactionScopeAsyncFlowOption.Enabled);
        }
    }
}