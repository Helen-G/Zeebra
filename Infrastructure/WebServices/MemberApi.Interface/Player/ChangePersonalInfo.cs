using System;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.MemberApi.Interface.Player
{
    public class ChangePersonalInfoRequest 
    {
        public Guid PlayerId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class ChangePersonalInfoResponse 
    {
    }
}