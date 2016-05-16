using System;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.Data;

namespace AFT.RegoV2.ApplicationServices.Payment
{
    public class PlayerBankAccountQueries : MarshalByRefObject, IApplicationService
    {
        private readonly IPaymentRepository _repository;

        public PlayerBankAccountQueries(IPaymentRepository repository)
        {
            _repository = repository;
        }

        [Permission(Permissions.View, Module = Modules.PlayerBankAccount)]
        public IQueryable<PlayerBankAccount> GetPlayerBankAccounts()
        {
            return _repository.PlayerBankAccounts
                .Include(x => x.Player.CurrentBankAccount)
                .Include(x => x.Bank)
                .AsQueryable();
        }

        [Permission(Permissions.View, Module = Modules.PlayerBankAccount)]
        public IQueryable<PlayerBankAccount> GetPlayerBankAccounts(PlayerId playerId)
        {
            return GetPlayerBankAccounts()
                .Where(x => x.Player.Id == playerId)
                .AsQueryable();
        }

        [Permission(Permissions.View, Module = Modules.PlayerBankAccount)]
        public IQueryable<PlayerBankAccount> GetPendingPlayerBankAccounts()
        {
            return GetPlayerBankAccounts()
                .Where(x => x.Status == BankAccountStatus.Pending)
                .AsQueryable();
        }
    }
}
