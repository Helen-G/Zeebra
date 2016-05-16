using System;

namespace AFT.RegoV2.Core.Payment.Data
{
    public class PlayerId
    {
        private readonly Guid _id;

        public PlayerId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(PlayerId id)
        {
            return id._id;
        }

        public static implicit operator PlayerId(Guid id)
        {
            return new PlayerId(id);
        }
    }

    public class Player
    {
        public Guid Id { get; set; }
        public string DomainName { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Address { get; set; }
        public string ZipCode { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string CurrencyCode { get; set; }
        public Guid BrandId { get; set; }
        public RegoV2.Domain.Payment.Data.Brand Brand { get; set; }
        public Guid VipLevelId { get; set; }
        public bool HousePlayer { get; set; }
        public bool? ExemptWithdrawalVerification { get; set; }
        public DateTimeOffset? ExemptWithdrawalFrom { get; set; }
        public DateTimeOffset? ExemptWithdrawalTo { get; set; }
        public int? ExemptLimit { get; set; }
        public DateTimeOffset DateRegistered { get; set; }
        public PlayerBankAccount CurrentBankAccount { get; set; }

        public string GetFullName()
        {
            return this.FirstName + " " + this.LastName;
        }
    }
}
