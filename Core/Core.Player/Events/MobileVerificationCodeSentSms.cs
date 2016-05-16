using System;
using AFT.RegoV2.Core.Common.Interfaces;

namespace AFT.RegoV2.Core.Player.Events
{
    public class MobileVerificationCodeSentSms : DomainEventBase
    {
        public Guid PlayerId { get; set; }
        public string VerificationCode { get; set; }

        public MobileVerificationCodeSentSms() { }

        public MobileVerificationCodeSentSms(Guid playerId, string verificationCode)
        {
            PlayerId = playerId;
            VerificationCode = verificationCode;
        }
    }
}
