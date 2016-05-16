using System;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using AFT.RegoV2.ApplicationServices.Player;
using AFT.RegoV2.Core.Brand.ApplicationServices;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Common.Data.Wallet;
using AFT.RegoV2.Core.Common.Interfaces;
using AFT.RegoV2.Core.Domain.Player;
using AFT.RegoV2.Core.Domain.Player.Data;
using AFT.RegoV2.Core.Game.ApplicationServices;
using AFT.RegoV2.Core.Player.Data;
using AFT.RegoV2.Core.Security.Events;
using AFT.RegoV2.Core.ApplicationServices.Player;
using AFT.RegoV2.Core.Player;
using AFT.RegoV2.MemberApi.Interface.Player;
using AutoMapper;
using ServiceStack.Validation;
using Player = AFT.RegoV2.Core.Player.Data.Player;

namespace AFT.RegoV2.MemberApi.Controllers
{
    public class PlayerController : BaseApiController
    {
        private readonly IPlayerRepository _repository;
        private readonly PlayerCommands _commands;
        private readonly IPlayerQueries _queries;
        private readonly BrandQueries _brandQueries;
        private readonly WalletQueries _walletQueries;
        private readonly IEventBus _eventBus;

        static PlayerController()
        {
            CreateDataMappings();
        }

        public PlayerController(IPlayerRepository repository,  PlayerCommands commands, IPlayerQueries queries,
            BrandQueries brandQueries, WalletQueries walletQueries, IEventBus eventBus)
        {
            _repository = repository;
            _brandQueries = brandQueries;
            _commands = commands;
            _queries = queries;
            _walletQueries = walletQueries;
            _eventBus = eventBus;
        }

        private static void CreateDataMappings()
        {
            Mapper.CreateMap<Player, EditPlayerData>()
                .ForMember(p => p.DateOfBirth, opt => opt.ResolveUsing(a => a.DateOfBirth.ToString("yyyy'/'MM'/'dd")));

            Mapper.CreateMap<Player, ProfileResponse>()
                .ForMember(p => p.CountryCode, opt => opt.MapFrom(a => a.CountryCode))
                .ForMember(p => p.CurrencyCode, opt => opt.MapFrom(a => a.CurrencyCode));

            Mapper.CreateMap<ChangePasswordRequest, ChangePasswordData>();

            Mapper.CreateMap<RegisterRequest, RegistrationData>()
                .ForMember(x => x.AccountStatus, y => y.Ignore())
                .ForMember(x => x.IdStatus, y => y.Ignore())
                .ForMember(x => x.IsRegisteredFromAdminSite, y => y.Ignore());
            Mapper.CreateMap<PlayerBalance, BalancesResponse>();

            Mapper.CreateMap<ChangePersonalInfoRequest, EditPlayerData>();
            Mapper.CreateMap<ChangeContactInfoRequest, EditPlayerData>();
        }

        [AllowAnonymous]
        public RegisterResponse Register(RegisterRequest request)
        {

            var registerData = Mapper.Map<RegisterRequest, RegistrationData>(request);

            var userId = _commands.Register(registerData);

            return new RegisterResponse
            {
                UserId = userId
            };
        }

        [AllowAnonymous]
        public ActivationResponse Activate(ActivationRequest request)
        {
            var token = request.Token;
            var activated = _commands.ActivateViaEmail(token);
            return new ActivationResponse()
            {
                Activated = activated,
                Token = token
            };
        }

        [HttpPost]
            //todo: explain in comments what is going on here and how this method is related to AuthServerProvider.GrantResourceOwnerCredentials
        public LogoutResponse Logout(LogoutRequest request)
        {
           
            Request.GetOwinContext().Authentication.SignOut(); // this doesn't remove cookies, so it's useless. :((

            return new LogoutResponse
            {
                UserName = Username
            };
        }

        [HttpGet]
        public ProfileResponse Profile()
        {
            var player = _repository.Players.Single(x => x.Id == PlayerId);

            var question = _repository.SecurityQuestions.FirstOrDefault(q => q.Id == player.SecurityQuestionId);

            var profileResponse = Mapper.Map<ProfileResponse>(player);
            if (question != null)
            {
                profileResponse.SecurityQuestion = question.Question;
            }

            return profileResponse;
        }

        [HttpGet]
        public SecurityQuestionsResponse SecurityQuestions()
        {
            return new SecurityQuestionsResponse
            {
                SecurityQuestions = _repository.SecurityQuestions.AsQueryable().AsNoTracking().ToList(),
            };
        }

        [HttpPost]
        public ChangePasswordResponse ChangePassword(ChangePasswordRequest request)
        {
            var requestData = Mapper.Map<ChangePasswordRequest, ChangePasswordData>(request);

            _commands.ChangePassword(requestData);

            return new ChangePasswordResponse();
        }


        [HttpPost]
        public ChangePersonalInfoResponse ChangePersonalInfo(ChangePersonalInfoRequest request)
        {
            var playerData = Mapper.Map<EditPlayerData>(_queries.GetPlayer(request.PlayerId));
            var newData = Mapper.Map<EditPlayerData>(request);
            playerData.PlayerId = request.PlayerId;
            playerData.Title = newData.Title;
            playerData.FirstName = newData.FirstName;
            playerData.LastName = newData.LastName;
            playerData.Email = newData.Email;
            playerData.DateOfBirth = newData.DateOfBirth;
            playerData.Gender = newData.Gender;
            playerData.CurrencyCode = newData.CurrencyCode;
            _commands.Edit(playerData);
            return new ChangePersonalInfoResponse ();
        }

        [HttpPost]
        public ChangeContactInfoResponse ChangeContactInfo(ChangeContactInfoRequest request)
        {
            var playerData = Mapper.Map<EditPlayerData>(_queries.GetPlayer(request.PlayerId));
            var newData = Mapper.Map<EditPlayerData>(request);
            playerData.PlayerId = request.PlayerId;
            playerData.PhoneNumber = newData.PhoneNumber;
            playerData.MailingAddressLine1 = newData.MailingAddressLine1;
            playerData.MailingAddressLine2 = newData.MailingAddressLine2;
            playerData.MailingAddressLine3 = newData.MailingAddressLine3;
            playerData.MailingAddressLine4 = newData.MailingAddressLine4;
            playerData.MailingAddressCity = newData.MailingAddressCity;
            playerData.MailingAddressPostalCode = newData.MailingAddressPostalCode;
            playerData.CountryCode = newData.CountryCode;
            playerData.ContactPreference = newData.ContactPreference;
            _commands.Edit(playerData);
            return new ChangeContactInfoResponse ();
        }

        [HttpPost]
        public ChangeSecurityQuestionResponse ChangeSecurityQuestion(ChangeSecurityQuestionRequest request)
        {
            var questionId = new Guid(request.SecurityQuestionId);
            _commands.ChangeSecurityQuestion(Guid.Parse(request.Id), questionId, request.SecurityAnswer);

            return new ChangeSecurityQuestionResponse ();
        }

        [AllowAnonymous]
        [HttpGet]
        public RegistrationFormDataResponse RegistrationFormData([FromUri]RegistrationFormDataRequest request)
        {
            var countries = _brandQueries.GetCountriesByBrand(request.BrandId).Select(a => a.Code).ToArray();
            var currencies = _brandQueries.GetCurrenciesByBrand(request.BrandId).Select(a => a.Code).ToArray();
            var genders = Enum.GetNames(typeof(Gender));
            var titles = Enum.GetNames(typeof(Title));
            var contactMethods = Enum.GetNames(typeof(ContactMethod));
            var questions = _repository.SecurityQuestions.AsQueryable().AsNoTracking().ToList();
            return new RegistrationFormDataResponse
            {
                CountryCodes = countries,
                CurrencyCodes = currencies,
                Genders = genders,
                Titles = titles,
                ContactMethods = contactMethods,
                SecurityQuestions = questions
            };
        }

        [AllowAnonymous]
        [HttpGet]
        public LanguagesResponse Languages([FromUri]LanguagesRequest request)
        {
            var languages = _brandQueries.GetCulturesByBrand(request.BrandId).Select(c => new Language
            {
                Culture = c.Code,
                NativeName = c.NativeName
            }).ToList();
            return new LanguagesResponse
            {
                Languages = languages
            };
        }

        [HttpPost]
        public ReferFriendsResponse ReferFriends(ReferFriendsRequest request)
        {
            _commands.ReferFriends(new ReferralData { ReferrerId = PlayerId, PhoneNumbers = request.PhoneNumbers });
            return new ReferFriendsResponse();
        }

        [HttpPost]
        public VerificationCodeResponse VerificationCode(VerificationCodeRequest request)
        {
            _commands.SendMobileVerificationCode(PlayerId);

            return new VerificationCodeResponse();
        }

        [HttpPost]
        public VerifyMobileResponse VerifyMobile(VerifyMobileRequest request)
        {
            _commands.VerifyMobileNumber(PlayerId, request.VerificationCode);

            return new VerifyMobileResponse();
        }

        [HttpGet]
        public BalancesResponse Balances([FromUri]BalancesRequest request)
        {
            var balance = _walletQueries.GetPlayerBalance(PlayerId, request.WalletId);

            return Mapper.Map<BalancesResponse>(balance);

        }

        [HttpPost]
        public PlayerData GetPlayerData(GetPlayerDataRequest request)
        {
            var player = _queries.GetPlayerByUsername(request.UserName);
            return new PlayerData
            {
                FirstName = player.FirstName,
                LastName = player.LastName
            };
        }
    }
}