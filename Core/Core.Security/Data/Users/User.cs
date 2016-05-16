using System;
using System.Collections.Generic;
using System.Linq;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Security.Data
{
    public enum UserStatus
    {
        Active,
        Inactive
    }

    public static class UserStatusExtension
    {
        public static string ToString(this UserStatus status)
        {
            switch (status)
            {
                case UserStatus.Active:
                    return "Active";

                case UserStatus.Inactive:
                    return "Inacvtive";

                default:
                    throw new RegoException("Unknown UserStatus value");
            }
        }
    }

    public class UserId
    {
        private readonly Guid _id;

        public UserId(Guid id)
        {
            _id = id;
        }

        public static implicit operator Guid(UserId id)
        {
            return id._id;
        }

        public static implicit operator UserId(Guid id)
        {
            return new UserId(id);
        }
    }

    public class User
    {
        public User()
        {
            Licensees = new List<UserLicenseeId>();
            AllowedBrands = new List<BrandId>();
            Currencies = new List<CurrencyCode>();
            BrandFilterSelections = new List<BrandFilterSelection>();
            LicenseeFilterSelections = new List<LicenseeFilterSelection>();
        }

        public Guid Id { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Language { get; set; }

        public UserStatus Status { get; set; }

        public string PasswordEncrypted { get; set; }

        public string Description { get; set; }

        public ICollection<UserLicenseeId> Licensees { get; set; }

        public Role Role { get; set; }

        public ICollection<BrandId> AllowedBrands { get; set; }

        public ICollection<CurrencyCode> Currencies { get; set; }

        public ICollection<BrandFilterSelection> BrandFilterSelections { get; set; }

        public ICollection<LicenseeFilterSelection> LicenseeFilterSelections { get; set; }
    }
}
