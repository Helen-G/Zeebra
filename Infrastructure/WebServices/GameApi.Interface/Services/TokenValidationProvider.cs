using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Common.Data;
using AFT.RegoV2.Core.Game.Exceptions;
using AFT.RegoV2.Core.Game.Extensions;
using AFT.RegoV2.GameApi.Interface.ServiceContracts;
using AFT.RegoV2.Infrastructure.Providers;

namespace AFT.RegoV2.GameApi.Interface.Services
{
    public interface ITokenValidationProvider
    {
        bool HasValidIp(TokenData token, string playerIpAddress);
        bool IsExpired(TokenData token, Type requestType);
        void ValidateToken(TokenData token, string userIp, Type requestType);
    }
    public sealed class TokenValidationProvider : ITokenValidationProvider
    {
        private const int DefaultPrimaryTokenExpirationHours = 24; // expiration for PlaceBet, GetBalance, ValidateToken
        private const int DefaultSecondaryTokenExpirationHours = 48; // expiration for WinBet, LoseBet and other operations

        private readonly int _primaryTokenExpirationSeconds;
        private readonly int _secondaryTokenExpirationSeconds;
        private static readonly HashSet<Type> ShortTimeOperations = 
            new HashSet<Type>
            {
                typeof(ValidateToken),
                typeof(GetBalance),
                typeof(PlaceBet)
            };

        private readonly ITokenValidationProvider _this;
        private readonly IWebConfigProvider _config;

        public TokenValidationProvider(IWebConfigProvider config)
        {
            _this = this;
            _config = config;

            _primaryTokenExpirationSeconds = 
                ParseSecondsFromConfig("PrimaryTokenExpirationHours", DefaultPrimaryTokenExpirationHours);
            _secondaryTokenExpirationSeconds = 
                ParseSecondsFromConfig("SecondaryTokenExpirationHours", DefaultSecondaryTokenExpirationHours);
        }

        bool ITokenValidationProvider.HasValidIp(TokenData token, string playerIpAddress)
        {
            var skipIpValidationSetting = _config.GetAppSettingByKey("SkipIpValidation");
            if (skipIpValidationSetting != null && Convert.ToBoolean(skipIpValidationSetting))
            {
                return true;
            }
            return token.PlayerIpAddress == playerIpAddress;
        }
        bool ITokenValidationProvider.IsExpired(TokenData token, Type requestType)
        {
            var nowAsUnixTime = DateTime.UtcNow.ToUnixTimeSeconds();
            if (ShortTimeOperations.Contains(requestType))
            {
                var expirationShort = token.Time + _primaryTokenExpirationSeconds;
                return expirationShort < nowAsUnixTime;
            }
            var expirationLong = token.Time + _secondaryTokenExpirationSeconds;
            return expirationLong < nowAsUnixTime;
        }

        void ITokenValidationProvider.ValidateToken(TokenData token, string userIp, Type requestType)
        {
            if (userIp != null && !_this.HasValidIp(token, userIp))
            {
                throw new InvalidPlayerIpInTokenException();
            }
            if (_this.IsExpired(token, requestType))
            {
                throw new ExpiredTokenException();
            }
        }

        private int ParseSecondsFromConfig(string key, int defaultValue)
        {
            var value = _config.GetAppSettingByKey(key);
            double d;
            var hours =  value != null && Double.TryParse(value, out d) ? d : defaultValue;
            return (int)Math.Round(hours*60*60);
        }

        
    }
}