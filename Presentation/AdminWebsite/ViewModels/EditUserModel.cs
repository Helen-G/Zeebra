using System;
using System.Collections.Generic;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    public class EditUserModel
    {
        public Guid Id { get; set; }

        public string Username { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Language { get; set; }

        public string Status { get; set; }
        public string Password { get; set; }

        public string PasswordConfirmation { get; set; }

        public Guid? RoleId { get; set; }

        public string RoleName { get; set; }

        public string Description { get; set; }

        public IList<Guid> AssignedLicensees { get; set; }

        public IList<Guid> AllowedBrands { get; set; }

        public IList<string> Currencies { get; set; }

        public UserStatus GetStatus()
        {
            return GetStatus(Status);
        }

        public static UserStatus GetStatus(string status)
        {
            switch (status)
            {
                case "Active":
                    return UserStatus.Active;
                case "Inactive":
                    return UserStatus.Inactive;
                default:
                    throw new RegoException("Unknown status");
            }
        }
    }
}