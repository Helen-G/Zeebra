using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Domain.Player;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Fraud;
using AFT.RegoV2.Core.Fraud.ApplicationServices.Data;
using AFT.RegoV2.Core.Fraud.Data;
using AFT.RegoV2.Core.Payment.ApplicationServices;
using AFT.RegoV2.Core.Payment.Data;
using AFT.RegoV2.Core.Payment.Validators;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.IpRegulations;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Users;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.BoundedContexts.Payment.Data;
using AFT.RegoV2.Domain.Payment;
using AFT.RegoV2.Domain.Payment.ApplicationServices.Data;
using AFT.RegoV2.Domain.Payment.Commands;
using AFT.RegoV2.Domain.Payment.Data;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using Microsoft.Practices.Unity.InterceptionExtension;
using VipLevel = AFT.RegoV2.Core.Player.Data.VipLevel;
using VipLevelId = AFT.RegoV2.Core.Player.Data.VipLevelId;
using BrandId = AFT.RegoV2.Core.Brand.Data.BrandId;
using RiskLevel = AFT.RegoV2.Core.Fraud.Data.RiskLevel;
using Player = AFT.RegoV2.Core.Player.Data.Player;
using PlayerId = AFT.RegoV2.Core.Common.Data.Player.PlayerId;


namespace AFT.RegoV2.Infrastructure.Aspects
{
    public class BrandCheckAspect : IInterceptionBehavior
    {
        private readonly ISecurityProvider _securityProvider;
        private readonly ISecurityRepository _securityRepository;
        private readonly IPaymentRepository _paymentRepository;
        private readonly IFraudRepository _fraudRepository;
        private readonly IPlayerRepository _playerRepository;

        protected ISecurityProvider SecurityProvider
        {
            get { return _securityProvider; }
        }

        public BrandCheckAspect(
            ISecurityProvider securityProvider,
            ISecurityRepository securityRepository,
            IPaymentRepository paymentRepository,
            IFraudRepository fraudRepository,
            IPlayerRepository playerRepository)
        {
            if (securityRepository == null)
                throw new ArgumentNullException("securityRepository");
            if (paymentRepository == null)
                throw new ArgumentNullException("paymentRepository");
            if (fraudRepository == null)
                throw new ArgumentNullException("fraudRepository");
            if (playerRepository == null)
                throw new ArgumentNullException("playerRepository");

            _securityProvider = securityProvider;
            _securityRepository = securityRepository;
            _paymentRepository = paymentRepository;
            _fraudRepository = fraudRepository;
            _playerRepository = playerRepository;
        }

        private IList<Guid> GetBrandIdsOrNull(IEnumerable arguments)
        {
            var result = new List<Guid>();

            foreach (var argument in arguments)
            {
                if (argument == null)
                    continue;

                #region Payment

                if (argument.GetType() == typeof(AddBankAccountData))
                    if (_paymentRepository.Banks.Any(o => o.Id == ((AddBankAccountData)argument).Bank))
                        result.Add(_paymentRepository.Banks.Single(o => o.Id == ((AddBankAccountData)argument).Bank).BrandId);

                if (argument.GetType() == typeof(EditBankAccountData))
                    if (_paymentRepository.Banks.Any(o => o.Id == ((EditBankAccountData)argument).Bank))
                        result.Add(_paymentRepository.Banks.Single(o => o.Id == ((EditBankAccountData)argument).Bank).BrandId);

                if (argument.GetType() == typeof(Domain.Payment.Data.BankAccountId))
                    if (_paymentRepository.BankAccounts.Any(b => b.Id == (Domain.Payment.Data.BankAccountId)argument) &&
                        _paymentRepository.BankAccounts.Single(b => b.Id == (Domain.Payment.Data.BankAccountId)argument).Bank != null)
                        result.Add(_paymentRepository.BankAccounts.Single(b => b.Id == (Domain.Payment.Data.BankAccountId)argument).Bank.BrandId);

                if (argument.GetType() == typeof(EditPlayerBankAccountCommand))
                    if (_paymentRepository.Banks.Any(o => o.Id == ((EditPlayerBankAccountCommand)argument).Bank))
                        result.Add(_paymentRepository.Banks.Single(o => o.Id == ((EditPlayerBankAccountCommand)argument).Bank).BrandId);

                if (argument.GetType() == typeof(PlayerBankAccountId))
                    if (_paymentRepository.PlayerBankAccounts.Any(b => b.Id == (PlayerBankAccountId)argument) &&
                        _paymentRepository.PlayerBankAccounts.Single(b => b.Id == (PlayerBankAccountId)argument).Bank != null)
                        result.Add(_paymentRepository.PlayerBankAccounts.Single(b => b.Id == (PlayerBankAccountId)argument).Bank.BrandId);

                if (argument.GetType() == typeof(Exemption))
                    if (_paymentRepository.Players.Any(o => o.Id == ((Exemption)argument).PlayerId))
                        result.Add(_paymentRepository.Players.Single(o => o.Id == ((Exemption)argument).PlayerId).BrandId);

                if (argument.GetType() == typeof(OfflineWithdrawRequest))
                    if (_paymentRepository.PlayerBankAccounts.Any(b => b.Id == ((OfflineWithdrawRequest)argument).PlayerBankAccountId) &&
                        _paymentRepository.PlayerBankAccounts.Single(b => b.Id == ((OfflineWithdrawRequest)argument).PlayerBankAccountId).Bank != null)
                        result.Add(_paymentRepository.PlayerBankAccounts.Single(b => b.Id == ((OfflineWithdrawRequest)argument).PlayerBankAccountId).Bank.BrandId);

                if (argument.GetType() == typeof(OfflineWithdrawId))
                    if (_paymentRepository.OfflineWithdraws.Any(b => b.Id == (OfflineWithdrawId)argument))
                        result.Add(_paymentRepository.OfflineWithdraws.Include(x => x.PlayerBankAccount.Bank).Single(b => b.Id == ((OfflineWithdrawId)argument)).PlayerBankAccount.Bank.BrandId);

                if (argument.GetType() == typeof(OfflineDepositId))
                    if (_paymentRepository.OfflineDeposits.Any(o => o.Id == (OfflineDepositId)argument))
                        result.Add(_paymentRepository.OfflineDeposits.Single(o => o.Id == (OfflineDepositId)argument).BrandId);

                if (argument.GetType() == typeof(OfflineDepositRequest))
                    if (_paymentRepository.Players.Any(o => o.Id == ((OfflineDepositRequest)argument).PlayerId))
                        result.Add(_paymentRepository.Players.Single(o => o.Id == ((OfflineDepositRequest)argument).PlayerId).BrandId);

                if (argument.GetType() == typeof(OfflineDepositRequest))
                    if (_paymentRepository.BankAccounts.Any(o => o.Id == ((OfflineDepositRequest)argument).BankAccountId) &&
                        _paymentRepository.BankAccounts.Single(o => o.Id == ((OfflineDepositRequest)argument).BankAccountId).Bank != null)
                        result.Add(_paymentRepository.BankAccounts.Single(o => o.Id == ((OfflineDepositRequest)argument).BankAccountId).Bank.BrandId);

                if (argument.GetType() == typeof(OfflineDepositConfirm))
                    if (_paymentRepository.Banks.Any(o => o.Id == ((OfflineDepositConfirm)argument).BankId))
                        result.Add(_paymentRepository.Banks.Single(o => o.Id == ((OfflineDepositConfirm)argument).BankId).BrandId);

                if (argument.GetType() == typeof(OfflineDepositApprove))
                    if (_paymentRepository.OfflineDeposits.Any(o => o.Id == ((OfflineDepositApprove)argument).Id))
                        result.Add(_paymentRepository.OfflineDeposits.Single(o => o.Id == ((OfflineDepositApprove)argument).Id).BrandId);

                if (argument.GetType() == typeof(EditPaymentLevel))
                    if (_paymentRepository.PaymentLevels.Any(o => o.Id == ((EditPaymentLevel)argument).Id))
                        result.Add(_paymentRepository.PaymentLevels.Single(o => o.Id == ((EditPaymentLevel)argument).Id).BrandId);

                if (argument.GetType() == typeof(PaymentSettingsId))
                    if (_paymentRepository.PaymentSettings.Any(o => o.Id == (PaymentSettingsId)argument))
                        result.Add(_paymentRepository.PaymentSettings.Single(o => o.Id == (PaymentSettingsId)argument).BrandId);

                if (argument.GetType() == typeof(SavePaymentSettingsCommand))
                    result.Add(((SavePaymentSettingsCommand)argument).Brand);

                if (argument.GetType() == typeof(TransferSettingsId))
                    if (_paymentRepository.TransferSettings.Any(o => o.Id == (TransferSettingsId)argument))
                        result.Add(_paymentRepository.TransferSettings.Single(o => o.Id == (TransferSettingsId)argument).BrandId);

                if (argument.GetType() == typeof(SaveTransferSettingsCommand))
                    result.Add(((SaveTransferSettingsCommand)argument).Brand);

                if (argument.GetType() == typeof (PlayerId))
                {
                   
                    if (_playerRepository.Players.Any(o => o.Id == (PlayerId)argument))
                        result.Add(_playerRepository.Players.Single(o => o.Id == (PlayerId)argument).BrandId);
                }
                if (argument.GetType() == typeof (Core.Payment.Data.PlayerId))
                {
                    if (_paymentRepository.Players.Any(o => o.Id == (Core.Payment.Data.PlayerId)argument))
                        result.Add(_paymentRepository.Players.Single(o => o.Id == (Core.Payment.Data.PlayerId)argument).BrandId);
                }

                #endregion

                #region Fraud

                if (argument.GetType() == typeof(AVCConfigurationDTO))
                        result.Add(((AVCConfigurationDTO)argument).Brand);

                if (argument.GetType() == typeof(RiskLevelId))
                    if (_fraudRepository.RiskLevels.Any(o => o.Id == (RiskLevelId)argument))
                        result.Add(_fraudRepository.RiskLevels.Single(o => o.Id == (RiskLevelId)argument).BrandId);

                if (argument.GetType() == typeof(RiskLevel))
                    result.Add(((RiskLevel)argument).BrandId);

                if (argument.GetType() == typeof(WagerConfigurationId))
                    if (_fraudRepository.WagerConfigurations.Any(o => o.Id == (WagerConfigurationId)argument))
                        result.Add(_fraudRepository.WagerConfigurations.Single(o => o.Id == (WagerConfigurationId)argument).BrandId);

                if (argument.GetType() == typeof(WagerConfigurationDTO))
                    result.Add(((WagerConfigurationDTO)argument).BrandId);

                #endregion

                #region Brand

                if (argument.GetType() == typeof(BrandId))
                    result.Add(((BrandId)argument));

                if (argument.GetType() == typeof(VipLevelViewModel))
                    result.Add(((VipLevelViewModel)argument).Brand);

                #endregion

                #region Player

                if (argument.GetType() == typeof(PlayerId))
                {
                    if (_playerRepository.Players.Any(o => o.Id == (PlayerId)argument))
                        result.Add(_playerRepository.Players.Single(o => o.Id == (PlayerId)argument).BrandId);
                }

                if (argument.GetType() == typeof(Player))
                {
                    if (_playerRepository.Players.Any(o => o.Id == ((Player)argument).Id))
                        result.Add(_playerRepository.Players.Single(o => o.Id == ((Player)argument).Id).BrandId);
                }

                if (argument.GetType() == typeof(EditPlayerData))
                {
                    if (_playerRepository.Players.Any(o => o.Id == ((EditPlayerData)argument).PlayerId))
                        result.Add(_playerRepository.Players.Single(o => o.Id == ((EditPlayerData)argument).PlayerId).BrandId);
                }

                if (argument.GetType() == typeof(VipLevelId))
                {
                    if (_playerRepository.VipLevels.Any(o => o.Id == (VipLevelId)argument))
                        result.Add(_playerRepository.VipLevels.Single(o => o.Id == (VipLevelId)argument).BrandId);
                }

                if (argument.GetType() == typeof(VipLevel))
                {
                    if (_playerRepository.VipLevels.Any(o => o.Id == ((VipLevel)argument).Id))
                        result.Add(_playerRepository.VipLevels.Single(o => o.Id == ((VipLevel)argument).Id).BrandId);
                }

                if (argument.GetType() == typeof(RegistrationData))
                {
                    result.Add(new Guid(((RegistrationData)argument).BrandId));
                }

                #endregion Player

                #region Security

                if (argument.GetType() == typeof(AddBrandIpRegulationData))
                {
                    result.Add(((AddBrandIpRegulationData)argument).BrandId);
                }

                if (argument.GetType() == typeof(EditBrandIpRegulationData))
                {
                    result.Add(((EditBrandIpRegulationData)argument).BrandId);
                }

                if (argument.GetType() == typeof(BrandIpRegulationId))
                {
                    if (_securityRepository.BrandIpRegulations.Any(o => o.Id == (BrandIpRegulationId)argument))
                        result.Add(_securityRepository.BrandIpRegulations.Single(o => o.Id == ((BrandIpRegulationId)argument)).BrandId);
                }

                if (argument.GetType() == typeof(AddUserData) && ((AddUserData)argument).AllowedBrands != null)
                {
                    result.AddRange(((AddUserData)argument).AllowedBrands);
                }

                if (argument.GetType() == typeof(EditUserData) && ((EditUserData)argument).AllowedBrands != null)
                {
                    result.AddRange(((EditUserData)argument).AllowedBrands);
                }

                if (argument.GetType() == typeof(UserId))
                {
                    if (_securityRepository.Users.Any(o => o.Id == (UserId)argument))
                        result.AddRange((_securityRepository.Users.Single(o => o.Id == ((UserId)argument)).AllowedBrands.Select(b => b.Id)));
                }

                #endregion
            }

            return result.Count > 0 ? result.Distinct().ToList() : null;
        }

        public IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptionBehaviorDelegate getNext)
        {
            var brandIds = GetBrandIdsOrNull(input.Arguments);

            if (brandIds == null)
                return getNext()(input, getNext);

            if (!_securityProvider.IsUserAvailable)
            {
                throw new RegoException("User must be logged in");
            }

            var user = _securityRepository
                .Users
                .Include(u => u.AllowedBrands)
                .SingleOrDefault(u => u.Id == _securityProvider.User.UserId);

            var allowed = user.AllowedBrands.Count(brand => brandIds.Contains(brand.Id)) == brandIds.Count();

            if (!allowed)
            {
                throw new InsufficientPermissionsException(
                    string.Format("User \"{0}\" has insufficient permissions for the operation ",
                            _securityProvider.User.UserName));
            }

            // Invoke the next behavior in the chain.
            var result = getNext()(input, getNext);

            return result;
        }

        public IEnumerable<Type> GetRequiredInterfaces()
        {
            return Type.EmptyTypes;
        }

        public bool WillExecute
        {
            get { return true; }
        }
    }
}
