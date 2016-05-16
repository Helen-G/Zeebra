using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Transactions;
using System.Web.Configuration;
using System.Web.WebSockets;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.ApplicationServices.Security;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common;
using AFT.RegoV2.Core.Common.Commands;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Content;
using AFT.RegoV2.Core.Common.Data.Content.MessageTemplateModels;
using AFT.RegoV2.Core.Common.Data.Player;
using AFT.RegoV2.Core.Common.Data.Brand;
using AFT.RegoV2.Core.Common.Events;
using AFT.RegoV2.Core.Common.Events.Player;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Common.Utils;
using AFT.RegoV2.Core.Content.ApplicationServices;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Domain.Player.Events;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Player.Enums;
using AFT.RegoV2.Core.Player.Events;
using AFT.RegoV2.Core.Player.Validators;
using AFT.RegoV2.Core.Security.Common;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Core.Services.Player.Validators;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Domain.BoundedContexts.Player.Data;
using AFT.RegoV2.Domain.BoundedContexts.Player.Validators;
using AFT.RegoV2.Domain.BoundedContexts.Security.Helpers;
using AFT.RegoV2.Domain.Player.Resources;
using AFT.RegoV2.Domain.Security.Interfaces;
using AFT.RegoV2.Shared;
using AutoMapper;
using FluentValidation.Results;
using ServiceStack.Validation;
using VipLevel = AFT.RegoV2.Core.Player.Data.VipLevel;
using VipLevelId = AFT.RegoV2.Core.Player.Data.VipLevelId;

namespace AFT.RegoV2.Core.ApplicationServices.Player
{
    public class PlayerCommands : MarshalByRefObject, IApplicationService
    {
        private readonly BrandQueries _brandQueries;
        private readonly IPlayerRepository _repository;
        private readonly IPlayerQueries _queries;
        private readonly IMessageTemplateService _messageTemplateService;
        private readonly IMessageTemplatesCommands _messageTemplatesCommands;
        private readonly IServiceBus _serviceBus;
        private readonly IEventBus _eventBus;
        private readonly ISecurityProvider _securityProvider;
        private readonly IFileStorage _fileStorage;

        static PlayerCommands()
        {
            Mapper.CreateMap<RegistrationData, Core.Player.Data.Player>()
                .ForMember(x => x.DateOfBirth, y => y.MapFrom(z => Convert.ToDateTime(z.DateOfBirth)))
                .ForMember(x => x.BrandId, y => y.MapFrom(z => new Guid(z.BrandId)))
                .ForMember(x => x.Gender, y => y.MapFrom(z => Enum.Parse(typeof(Gender), z.Gender)))
                .ForMember(x => x.Title, y => y.MapFrom(z => Enum.Parse(typeof(Title), z.Title)))
                .ForMember(x => x.ContactPreference,
                    y => y.MapFrom(z => Enum.Parse(typeof(ContactMethod), z.ContactPreference)))
                .ForMember(x => x.IdStatus, y => y.MapFrom(z => Enum.Parse(typeof(IdStatus), z.IdStatus)))
                .ForMember(x => x.AccountStatus, y => y.MapFrom(z => Enum.Parse(typeof(AccountStatus), z.AccountStatus)))
                .ForMember(x => x.SecurityQuestionId,
                    y => y.MapFrom(z => string.IsNullOrEmpty(z.SecurityQuestionId) ? (Guid?)null : new Guid(z.SecurityQuestionId)))
                .ForMember(x => x.ReferralId,
                    y => y.MapFrom(z => string.IsNullOrEmpty(z.ReferralId) ? (Guid?)null : new Guid(z.ReferralId)));

            Mapper.CreateMap<EditPlayerData, Core.Player.Data.Player>()
                .ForMember(x => x.DateOfBirth, y => y.MapFrom(z => Convert.ToDateTime(z.DateOfBirth)));

            Mapper.CreateMap<Core.Player.Data.Player, PlayerInfoLog>();
        }

        public PlayerCommands(
            IPlayerRepository repository,
            IServiceBus serviceBus,
            IEventBus eventBus,
            IMessageTemplateService messageTemplateService,
            BrandQueries brandQueries,
            ISecurityProvider securityProvider,
            IPlayerQueries queries,
            IFileStorage fileStorage,
            IMessageTemplatesCommands messageTemplatesCommands)
        {
            _repository = repository;
            _serviceBus = serviceBus;
            _eventBus = eventBus;
            _brandQueries = brandQueries;
            _messageTemplateService = messageTemplateService;
            _securityProvider = securityProvider;
            _queries = queries;
            _fileStorage = fileStorage;
            _messageTemplatesCommands = messageTemplatesCommands;
        }

        public void SendNewPassword(SendNewPasswordData request)
        {
            var validationResult = new SendNewPasswordValidator(_repository).Validate(request);

            if (!validationResult.IsValid)
            {
                throw validationResult.GetValidationError();
            }
            var newPassword = request.NewPassword;
            if (string.IsNullOrWhiteSpace(newPassword))
            {
                newPassword = PasswordGenerator.Create();
            }
            var player = _repository.Players.FirstOrDefault(p => p.Id == request.PlayerId);
            if (player == null)
                throw new RegoException(string.Format("Can't find Player with Id = {0}", request.PlayerId));

            player.PasswordEncrypted = PasswordHelper.EncryptPassword(request.PlayerId, newPassword);
            _repository.SaveChanges();

            var brand = _brandQueries.GetBrandOrNull(player.BrandId);
            if (request.SendBy == SendBy.Email)
                SendEmailNewPassword(brand.Name, player.Email, player.Username, newPassword);
            else
                SendSmsNewPassword(brand.Name, player.PhoneNumber, player.Username, newPassword);
        }

        public PlayerCommandResult Login(string username, string password, LoginRequestContext context)
        {
            if (null == context.BrandId)
                throw new ArgumentNullException("BrandId");

            if (string.IsNullOrWhiteSpace(context.IpAddress))
                throw new ArgumentNullException("IpAddress");

            if (null == context.BrowserHeaders)
                throw new ArgumentNullException("BrowserHeaders");

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var result = _queries.ValidateLogin(username, password);

                if (result.Success)
                {
                    result.Player.FailedLoginAttempts = 0;
                    _eventBus.Publish(new MemberAuthenticationSucceded
                    {
                        BrandId = context.BrandId,
                        Username = username,
                        IPAddress = context.IpAddress,
                        Headers = context.BrowserHeaders
                    });
                }
                else
                {
                    var validationResult = result.ValidationResult;
                    var player = result.Player;

                    LockPlayerOnTooManyFailedLoginAttempts(player, validationResult);

                    _eventBus.Publish(new MemberAuthenticationFailed
                    {
                        BrandId = context.BrandId,
                        Username = username,
                        IPAddress = context.IpAddress,
                        Headers = context.BrowserHeaders,
                        FailReason = string.Join(";", validationResult.Errors.Select(x => x.ErrorMessage))
                    });
                }

                _repository.SaveChanges();
                scope.Complete();

                return result;
            }
        }


        private static void LockPlayerOnTooManyFailedLoginAttempts(Core.Player.Data.Player player, ValidationResult validationResult)
        {
            if (player != null
                &&
                validationResult.Errors.Any(
                    x => x.ErrorMessage == PlayerAccountResponseCode.UsernamePasswordCombinationIsNotValid.ToString()))
            {
                player.FailedLoginAttempts++;

                var maxFailedLoginAttempts =
                    Convert.ToInt32(ConfigurationManager.AppSettings["MaxFailedLoginAttempts"]);

                if (player.FailedLoginAttempts >= maxFailedLoginAttempts)
                    player.AccountStatus = AccountStatus.Locked;
            }
        }

        [Permission(Permissions.Edit, Module = Modules.PlayerManager)]
        public void Edit(EditPlayerData request)
        {
            var validationResult = new EditPlayerValidator(_repository, _brandQueries).Validate(request);

            if (!validationResult.IsValid)
            {
                throw validationResult.GetValidationError();
            }

            var player = _repository.Players.Include(p => p.VipLevel).FirstOrDefault(p => p.Id == request.PlayerId);

            if (player == null)
                throw new RegoException(string.Format("Can't find Player with Id = {0}", request.PlayerId));

            var r = Mapper.Map<Core.Player.Data.Player>(request);

            player.FirstName = r.FirstName;
            player.LastName = r.LastName;
            player.DateOfBirth = r.DateOfBirth;
            player.Title = r.Title;
            player.Gender = r.Gender;
            player.Email = r.Email;
            player.PhoneNumber = r.PhoneNumber;
            player.MailingAddressLine1 = r.MailingAddressLine1;
            player.MailingAddressLine2 = r.MailingAddressLine2;
            player.MailingAddressLine3 = r.MailingAddressLine3;
            player.MailingAddressLine4 = r.MailingAddressLine4;
            player.MailingAddressCity = r.MailingAddressCity;
            player.MailingAddressPostalCode = r.MailingAddressPostalCode;
            player.PhysicalAddressLine1 = r.PhysicalAddressLine1;
            player.PhysicalAddressLine2 = r.PhysicalAddressLine2;
            player.PhysicalAddressLine3 = r.PhysicalAddressLine3;
            player.PhysicalAddressLine4 = r.PhysicalAddressLine4;
            player.PhysicalAddressCity = r.PhysicalAddressCity;
            player.PhysicalAddressPostalCode = r.PhysicalAddressPostalCode;
            player.CountryCode = r.CountryCode;
            player.ContactPreference = r.ContactPreference;
            player.AccountAlertEmail = r.AccountAlertEmail;
            player.AccountAlertSms = r.AccountAlertSms;
            player.MarketingAlertEmail = r.MarketingAlertEmail;
            player.MarketingAlertPhone = r.MarketingAlertPhone;
            player.MarketingAlertSms = r.MarketingAlertSms;

            _repository.SaveChanges();

            var country = _brandQueries.GetCountry(request.CountryCode);

            _eventBus.Publish(new PlayerUpdated
            {
                Player = new PlayerUpdated.PlayerData
                {
                    Id = player.Id,
                    PaymentLevelId = player.PaymentLevelId,
                    VipLevel = player.VipLevel != null ? player.VipLevel.Name : null,
                    VipLevelId = player.VipLevel != null ? player.VipLevel.Id : Guid.Empty,
                    DisplayName = GetFullName(player),
                    DateOfBirth = player.DateOfBirth,
                    Title = player.Title.ToString(),
                    Gender = player.Gender.ToString(),
                    Email = player.Email,
                    PhoneNumber = player.PhoneNumber,
                    CountryName = country.Name,
                    AddressLines = new[]
                    {
                        player.MailingAddressLine1,
                        player.MailingAddressLine2,
                        player.MailingAddressLine3,
                        player.MailingAddressLine4
                    },
                    ZipCode = player.MailingAddressPostalCode,
                    AccountAlertEmail = player.AccountAlertEmail,
                    AccountAlertSms = player.AccountAlertSms,
                    MarketingAlertEmail = player.MarketingAlertEmail,
                    MarketingAlertPhone = player.MarketingAlertPhone,
                    MarketingAlertSms = player.MarketingAlertSms
                }
            });
        }

        [Permission(Permissions.AssignVipLevel, Module = Modules.PlayerManager)]
        public void ChangePlayerVipLevel(VipLevelId oldVipLevelId, VipLevelId newVipLevelId)
        {
            var players = _repository.Players.Include(p => p.VipLevel)
                .Where(o => o.VipLevel.Id == oldVipLevelId);

            var vipLevel = _repository.VipLevels.Single(o => o.Id == newVipLevelId);

            foreach (var player in players)
                player.VipLevel = vipLevel;

            _repository.SaveChanges();

            foreach (var player in players)
            {
                var country = _brandQueries.GetCountry(player.CountryCode);
                _eventBus.Publish(new PlayerUpdated
                {
                    Player = new PlayerUpdated.PlayerData
                    {
                        Id = player.Id,
                        PaymentLevelId = player.PaymentLevelId,
                        VipLevel = player.VipLevel != null ? player.VipLevel.Name : null,
                        VipLevelId = player.VipLevel != null ? player.VipLevel.Id : Guid.Empty,
                        DisplayName = GetFullName(player),
                        DateOfBirth = player.DateOfBirth,
                        Title = player.Title.ToString(),
                        Gender = player.Gender.ToString(),
                        Email = player.Email,
                        PhoneNumber = player.PhoneNumber,
                        CountryName = country.Name,
                        AddressLines = new[]
                    {
                        player.MailingAddressLine1,
                        player.MailingAddressLine2,
                        player.MailingAddressLine3,
                        player.MailingAddressLine4
                    },
                        ZipCode = player.MailingAddressPostalCode,
                        AccountAlertEmail = player.AccountAlertEmail,
                        AccountAlertSms = player.AccountAlertSms,
                        MarketingAlertEmail = player.MarketingAlertEmail,
                        MarketingAlertPhone = player.MarketingAlertPhone,
                        MarketingAlertSms = player.MarketingAlertSms
                    }
                });
            }
        }

        [Permission(Permissions.AssignVipLevel, Module = Modules.PlayerManager)]
        public void AssignVip(Core.Player.Data.Player player, VipLevel vipLevel)
        {
            player.VipLevel = vipLevel;
        }

        public bool ActivateViaEmail(string emailActivationToken)
        {
            return Activate(emailActivationToken, ContactType.Email);
        }

        public bool ActivateViaSms(string smsActivationToken)
        {
            return Activate(smsActivationToken, ContactType.Mobile);
        }

        private bool Activate(string activationToken, ContactType contactType)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var player = contactType == ContactType.Email
                    ? _repository.Players.SingleOrDefault(x => x.AccountActivationEmailToken == activationToken)
                    : _repository.Players.SingleOrDefault(x => x.AccountActivationSmsToken == activationToken);

                if (player == null) return false;
                if (player.AccountStatus != AccountStatus.Inactive) return false;
                if (contactType == ContactType.Email && player.AccountActivationEmailToken == string.Empty) return false;
                if (contactType == ContactType.Mobile && player.AccountActivationSmsToken == string.Empty) return false;

                player.AccountStatus = AccountStatus.Active;
                player.AccountActivationEmailToken = string.Empty;
                player.AccountActivationSmsToken = string.Empty;

                if (contactType == ContactType.Email)
                    player.EmailVerificationToken = string.Empty;

                _repository.SaveChanges();

                _eventBus.Publish(new PlayerStatusChanged(player.Id, player.AccountStatus));
                _eventBus.Publish(new PlayerContactVerified(player.Id, contactType));

                scope.Complete();

                return true;
            }
        }

        public void ReferFriends(ReferralData referralData)
        {
            var validationResult = new ReferalDataValidator(_repository).Validate(referralData);

            if (validationResult.IsValid == false)
            {
                throw validationResult.GetValidationError();
            }

            _eventBus.Publish(new PlayersReferred(referralData.ReferrerId, referralData.PhoneNumbers));
        }

        [Permission(Permissions.Activate, Module = Modules.PlayerManager)]
        [Permission(Permissions.Deactivate, Module = Modules.PlayerManager)]
        public void SetStatus(PlayerId playerId, bool active)
        {
            var player = _repository.Players.SingleOrDefault(x => x.Id == playerId);
            if (player == null)
                throw new RegoException(string.Format("Can't find Player with Id = {0}", playerId));

            player.AccountStatus = active ? AccountStatus.Active : AccountStatus.Inactive;
            _repository.SaveChanges();
            _eventBus.Publish(new PlayerStatusChanged(playerId, player.AccountStatus));
        }

        public void ResendActivationEmail(Guid id)
        {
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead }))
            {
                var player = _repository.Players.Single(x => x.Id == id);
                player.AccountActivationEmailToken = Guid.NewGuid().ToString();
                _repository.SaveChanges();

                scope.Complete();

                var activationUrl = _brandQueries.GetBrandOrNull(player.BrandId).EmailActivationUrl;
                SendActivationEmail(player.Username, player.Email, activationUrl, player.AccountActivationEmailToken, player.Id, true);
            }
        }

        [Permission(Permissions.Add, Module = Modules.PlayerManager)]
        public Guid Register(RegistrationData request)
        {

            // validate the request
            var validationResult = new RegisterValidator(_repository, _brandQueries).Validate(request);

            if (!validationResult.IsValid)
                throw validationResult.GetValidationError();


            // prepare the Player data
            var player = Mapper.Map<Core.Player.Data.Player>(request);
            var playerActivationMethod = _brandQueries.GetPlayerActivationMethod(player.BrandId);

            var defaultVipLevel = _queries.GetDefaultVipLevel(player.BrandId);

            player.Id = Guid.NewGuid();
            player.PasswordEncrypted = PasswordHelper.EncryptPassword(player.Id, request.Password);
            player.VipLevelId = defaultVipLevel.Id;

            var defaultPaymentLevelId = _brandQueries.GetDefaultPaymentLevelId(player.BrandId, player.CurrencyCode);
            player.PaymentLevelId = defaultPaymentLevelId ?? Guid.Empty;
            player.AccountStatus = GetInitialAccountStatus(request, playerActivationMethod);
            player.DateRegistered = DateTimeOffset.UtcNow;
            player.EmailVerificationToken = Guid.NewGuid().ToString();
            player.IdentityVerifications = new Collection<IdentityVerification>();

            if (playerActivationMethod == PlayerActivationMethod.Email ||
                playerActivationMethod == PlayerActivationMethod.EmailOrSms)
                player.AccountActivationEmailToken = Guid.NewGuid().ToString();

            if (playerActivationMethod == PlayerActivationMethod.Sms ||
                playerActivationMethod == PlayerActivationMethod.EmailOrSms)
                player.AccountActivationSmsToken = new Random().Next(100000, 999999).ToString("D6");

            // Set marketing alerts by default, later could be confighured in Player Info
            player.MarketingAlertEmail = true;
            player.MarketingAlertPhone = true;
            player.MarketingAlertSms = true;

            var country = _brandQueries.GetCountry(player.CountryCode);
            var culture = _brandQueries.GetCulture(player.CultureCode);

            // open the scope here
            using (var scope = new TransactionScope(TransactionScopeOption.Required,
                new TransactionOptions { IsolationLevel = IsolationLevel.RepeatableRead }))
            {
                _repository.Players.Add(player);
                _repository.SaveChanges();

                _eventBus.Publish(new PlayerRegistered
                {
                    PlayerId = player.Id,
                    BrandId = player.BrandId,
                    CountryCode = player.CountryCode,
                    CountryName = country.Name,
                    VipLevel = defaultVipLevel.Code,
                    VipLevelId = defaultVipLevel.Id,
                    DateRegistered = player.DateRegistered,
                    DisplayName = GetFullName(player),
                    CurrencyCode = player.CurrencyCode,
                    PaymentLevelId = player.PaymentLevelId,
                    UserName = player.Username,
                    Email = player.Email,
                    PhoneNumber = player.PhoneNumber,
                    AccountActivationToken = player.AccountActivationEmailToken,
                    ReferralId = player.ReferralId,
                    IPAddress = player.IpAddress,
                    Title = player.Title.ToString(),
                    Gender = player.Gender.ToString(),
                    DateOfBirth = player.DateOfBirth,
                    AddressLines = new[]
                        {
                            player.MailingAddressLine1,
                            player.MailingAddressLine2,
                            player.MailingAddressLine3,
                            player.MailingAddressLine4
                        },
                    ZipCode = player.MailingAddressPostalCode,
                    Language = culture.Name,
                    CultureCode = culture.Code,
                    Status = player.AccountStatus.ToString(),
                    FirstName = player.FirstName,
                    LastName = player.LastName,
                    AccountAlertEmail = player.AccountAlertEmail,
                    AccountAlertSms = player.AccountAlertSms
                });

                SendActivationMessages(player, playerActivationMethod);

                scope.Complete();
            }

            return player.Id;
        }

        private AccountStatus GetInitialAccountStatus(RegistrationData registrationData, PlayerActivationMethod playerActivationMethod)
        {
            if (registrationData.IsRegisteredFromAdminSite)
                return (AccountStatus)Enum.Parse(typeof(AccountStatus), registrationData.AccountStatus);

            return playerActivationMethod == PlayerActivationMethod.Automatic
                ? AccountStatus.Active
                : AccountStatus.Inactive;
        }

        private void SendActivationMessages(Core.Player.Data.Player player, PlayerActivationMethod playerActivationMethod)
        {
            if (player.AccountStatus == AccountStatus.Active) return;

            var activationUrl = _brandQueries.GetBrandOrNull(player.BrandId).EmailActivationUrl;

            switch (playerActivationMethod)
            {
                case PlayerActivationMethod.Email:
                    SendActivationEmail(GetFullName(player), player.Email, activationUrl, player.AccountActivationEmailToken, player.Id);
                    break;
                case PlayerActivationMethod.Sms:
                    SendActivationSms(GetFullName(player), player.PhoneNumber, player.AccountActivationSmsToken, player.Id);
                    break;
                case PlayerActivationMethod.EmailOrSms:
                    SendActivationEmail(GetFullName(player), player.Email, activationUrl, player.AccountActivationEmailToken, player.Id);
                    SendActivationSms(GetFullName(player), player.PhoneNumber, player.AccountActivationSmsToken, player.Id);
                    break;
            }
        }

        private void SendActivationSms(
            string name,
            string mobileNumber,
            string token,
            Guid playerId)
        {
            var notificationModel = new ActivationSmsNotificationModel
            {
                Username = name,
                ActivationToken = token
            };

            var message = _messageTemplateService.GetMessage(MessageTemplateIdentifiers.ActivationSmsMessage,
                notificationModel);

            var commandMessage = new PlayerActivationSmsCommandMessage(mobileNumber, message, playerId, token);

            _serviceBus.PublishMessage(commandMessage);
        }

        private void SendEmailNewPassword(
            string brand,
            string email,
            string userName,
            string newPassword)
        {
            var notificationModel = new NewPasswordNotificationModel()
            {
                Username = userName,
                Newpassword = newPassword,
                Brand = brand
            };

            var message = _messageTemplateService.GetMessage(MessageTemplateIdentifiers.NewPasswordMessage, notificationModel);

            SendEmail(
                message,
                WebConfigurationManager.AppSettings["Smtp.From"],
                email,
                "New password",
                string.Format("{0} Customer Support", brand));

            _eventBus.Publish(new NewPasswordSent(userName, email));
        }

        private void SendActivationEmail(string name, string emailAddr, string activationUrl, string token,
            Guid playerId, bool isResentActivationEmail = false)
        {
            var notificationModel = new ActivationEmailNotificationModel
            {
                Username = name,
                ActivationLink = activationUrl + "?token=" + token
            };

            var message = _messageTemplateService.GetMessage(MessageTemplateIdentifiers.ActivationLinkMessage,
                notificationModel);

            var commandMessage = new EmailCommandMessage(
                "support@regov2.com",
                "Rego V2",
                emailAddr,
                name,
                "Account Activation",
                message);

            _serviceBus.PublishMessage(commandMessage);
        }

        private void SendSmsNewPassword(
            string brand,
            string phoneNumber,
            string userName,
            string newPassword)
        {
            var notificationModel = new NewPasswordNotificationModel()
            {
                Username = userName,
                Newpassword = newPassword,
                Brand = brand
            };

            var message =
                _messageTemplateService.GetMessage(MessageTemplateIdentifiers.NewPasswordMessage, notificationModel)
                    .Replace("<br/>", " ");
            SendSms(phoneNumber, message);
        }

        private void SendEmail(string body, string from, string to, string subject, string fromTitle, string toTitle = "")
        {
            var message = new EmailCommandMessage(from, fromTitle, to, toTitle, subject, body);

            _serviceBus.PublishMessage(message);
        }

        private void SendSms(
            string phoneNumber,
            string smsBody)
        {
            var smsMsg = new SmsCommandMessage(phoneNumber, smsBody);
            _serviceBus.PublishMessage(smsMsg);
        }

        [Permission(Permissions.AssignVipLevel, Module = Modules.PlayerManager)]
        public void ChangeVipLevel(PlayerId playerId, VipLevelId vipLevelId, string remarks)
        {
            var player = _repository.Players.SingleOrDefault(x => x.Id == playerId);
            if (player != null)
            {
                var vipLevel = _repository.VipLevels.SingleOrDefault(x => x.Id == vipLevelId);
                if (vipLevelId != null)
                {
                    player.VipLevel = vipLevel;
                    _repository.SaveChanges();
                    _eventBus.Publish(new PlayerVipLevelChanged(player.Id, vipLevel.Id));
                }
            }
        }

        public PlayerCommandResult ChangePassword(ChangePasswordData request)
        {
            var passwordMinLength = 6;
            var passwordMaxLength = 12;

            var validator = new ChangePasswordValidator(_repository, passwordMinLength, passwordMaxLength);

            var validationResult = validator.Validate(request);

            if (validationResult.IsValid == false)
                throw new RegoValidationException(validationResult);


            var loginValidationResult = _queries.ValidateLogin(request.Username, request.OldPassword);

            if (!loginValidationResult.Success)
            {
                throw new RegoValidationException(loginValidationResult.ValidationResult);
            }

            var player = loginValidationResult.Player;
            player.PasswordEncrypted = PasswordHelper.EncryptPassword(player.Id, request.NewPassword);

            var firstOrDefault = _repository.Players.FirstOrDefault(pl => pl.Username == player.Username);

            if (firstOrDefault != null)
                firstOrDefault.PasswordEncrypted = player.PasswordEncrypted;

            _repository.SaveChanges();

            return loginValidationResult;
        }

        public void ChangeSecurityQuestion(Guid playerId, Guid questionId, string answer)
        {
            var validationResult = new ChangeSecurityQuestionValidator(_repository).Validate(new ChangeSecurityQuestionData
            {
                Id = playerId.ToString(),
                SecurityAnswer = answer,
                SecurityQuestionId = questionId.ToString()
            });

            if (!validationResult.IsValid)
            {
                throw validationResult.GetValidationError();
            }

            var player = _repository.Players.SingleOrDefault(x => x.Id == playerId);
            if (player == null)
                throw new ValidationError(PlayerAccountResponseCode.PlayerDoesNotExist.ToString(), Messages.PlayerDoesNotExist);

            player.SecurityQuestionId = questionId;
            player.SecurityAnswer = answer;
            _repository.SaveChanges();

        }

        public void SendMobileVerificationCode(Guid playerId)
        {
            var player = _repository.Players.SingleOrDefault(p => p.Id == playerId);
            if (player == null) throw new RegoException("Player not found.");
            if (string.IsNullOrEmpty(player.PhoneNumber))
                throw new RegoException("No mobile number is available for the player.");
            if (player.IsPhoneNumberVerified) throw new RegoException("Mobile number is already verified.");
            player.MobileVerificationCode = new Random().Next(0, 9999);
            _repository.SaveChanges();

            SendSmsMobileVerificationCode(player.Id, player.PhoneNumber, GetFullName(player), player.MobileVerificationCode);
        }

        private void SendSmsMobileVerificationCode(Guid playerId, string phoneNumber, string name, int mobileVerificationCode)
        {
            var notificationModel = new MobileVerificationNotificationModel()
            {
                Username = name,
                VerificationCode = mobileVerificationCode.ToString("D4")
            };
            var message = _messageTemplateService.GetMessage(
                MessageTemplateIdentifiers.MobileVerificationMessage,
                notificationModel);

            SendSms(phoneNumber, message);

            _eventBus.Publish(new
                MobileVerificationCodeSentSms(playerId, mobileVerificationCode.ToString("D4")));
        }

        public void VerifyMobileNumber(Guid playerId, string verificationCode)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var player = _repository.Players.SingleOrDefault(p => p.Id == playerId);
                if (player == null) throw new RegoException("Player not found.");
                if (player.IsPhoneNumberVerified) throw new ArgumentException("Mobile number is already verified.");

                int code;
                var codeIsValid = int.TryParse(verificationCode, out code);
                if (codeIsValid == false) throw new ArgumentException("Verification code should be 4 digit number.");
                if (player.MobileVerificationCode != code) throw new ArgumentException("Verification code is incorrect.");

                player.IsPhoneNumberVerified = true;
                _repository.SaveChanges();

                _eventBus.Publish(new PlayerContactVerified(player.Id, ContactType.Mobile));
                scope.Complete();
            }
        }

        public void AddPlayerInfoLogRecord(Guid playerId)
        {
            var player = _repository.Players.Single(p => p.Id == playerId);
            var playerLogRecord = Mapper.Map<PlayerInfoLog>(player);
            playerLogRecord.Id = Guid.NewGuid();
            playerLogRecord.Player = player;
            _repository.PlayerInfoLog.Add(playerLogRecord);
            _repository.SaveChanges();
        }

        public void PlayerLogin(string username)
        {
            var player = _repository.Players.Single(p => p.Username == username);
            player.LastActivityDate = DateTimeOffset.Now;

            _repository.SaveChanges();
        }

        public void PlayerLogout(string username)
        {
            var player = _repository.Players.Single(p => p.Username == username);
            player.LastActivityDate = null;

            _repository.SaveChanges();
        }

        public void UpdateActivity(string username)
        {
            var player = _repository.Players.Single(p => p.Username == username);
            if (player.LastActivityDate.HasValue)
            {
                player.LastActivityDate = DateTimeOffset.Now;
            }

            _repository.SaveChanges();
        }


        public void UpdateLogRemark(Guid id, string remarks)
        {
            var log = _repository.PlayerActivityLog.SingleOrDefault(l => l.Id == id);
            if (log == null)
                throw new RegoException("Activity log record not found");

            log.Remarks = remarks;
            log.UpdatedBy = _securityProvider.User.UserName;
            log.DateUpdated = DateTimeOffset.Now;

            _repository.SaveChanges();
        }

        public void UpdatePlayersPaymentLevel(Guid currentPaymentLevelId, Guid newPaymentLevelId)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var players = _repository.Players.Where(x => x.PaymentLevelId == currentPaymentLevelId);

                foreach (var player in players)
                {
                    player.PaymentLevelId = newPaymentLevelId;
                }

                _repository.SaveChanges();

                scope.Complete();
            }
        }

        public string GetFullName(Core.Player.Data.Player player)
        {
            return player.FirstName + " " + player.LastName;
        }

        private string SaveFile(string fileName, string fileNameTemplate, byte[] content)
        {
            if (content != null && content.Length > 0)
            {
                var format = string.Format("{0}.xxx", fileNameTemplate);
                var newFileName = Path.ChangeExtension(format, Path.GetExtension(fileName));
                _fileStorage.Save(newFileName, content);
                return newFileName;
            }
            return null;
        }

        public IdentityVerification UploadIdentificationDocuments(IdUploadData uploadData, Guid playerId, string userName)
        {
            var player = _repository.Players
                .Include(o => o.IdentityVerifications)
                .Single(o => o.Id == playerId);

            if (player.IdentityVerifications
                .Any(o => o.DocumentType == uploadData.DocumentType
                    && o.VerificationStatus == VerificationStatus.Verified))
                throw new InvalidOperationException("Player already has verified document of this type.");

            IdentityVerification identity;
            var id = Guid.NewGuid();

            var idFrontImageName = SaveFile(uploadData.FrontName, string.Format("{0}-FrontId", id), uploadData.FrontIdFile);
            var idBackImageName = SaveFile(uploadData.BackName, string.Format("{0}-BackId", id), uploadData.BackIdFile);

            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                identity = new IdentityVerification
                {
                    Id = id,
                    CardNumber = uploadData.CardNumber,
                    DocumentType = uploadData.DocumentType,
                    ExpirationDate = uploadData.CardExpirationDate,
                    DateUploaded = DateTimeOffset.Now,
                    UploadedBy = userName,
                    VerificationStatus = VerificationStatus.Pending,
                    FrontFilename = idFrontImageName,
                    BackFilename = idBackImageName,
                    Remarks = uploadData.Remarks
                };
                player.IdentityVerifications.Add(identity);

                _repository.SaveChanges();

                scope.Complete();
            }

            return identity;
        }

        public void VerifyIdDocument(Guid id, string userName)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var identification = _repository.Players
                    .Include(o => o.IdentityVerifications)
                    .SelectMany(o => o.IdentityVerifications)
                    .Single(o => o.Id == id);

                identification.VerificationStatus = VerificationStatus.Verified;
                identification.DateVerified = DateTimeOffset.Now;
                identification.VerifiedBy = userName;

                _repository.SaveChanges();
                scope.Complete();
            }
        }

        public void UnverifyIdDocument(Guid id, string userName)
        {
            using (var scope = CustomTransactionScope.GetTransactionScope())
            {
                var identification = _repository.Players
                    .Include(o => o.IdentityVerifications)
                    .SelectMany(o => o.IdentityVerifications)
                    .Single(o => o.Id == id);

                identification.VerificationStatus = VerificationStatus.Unverified;
                identification.DateUnverified = DateTimeOffset.Now;
                identification.UnverifiedBy = userName;

                _repository.SaveChanges();
                scope.Complete();
            }
        }
    }

}