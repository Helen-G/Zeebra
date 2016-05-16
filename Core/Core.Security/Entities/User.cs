using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Security.Data;
using AFT.RegoV2.Core.Services.Security;
using AFT.RegoV2.Shared;

namespace AFT.RegoV2.Core.Security.Entities
{
    public class User
    {
        private readonly Data.User _data;

        public User(Data.User data)
        {
            _data = data;
        }

        public Data.User Data { get { return _data; } }

        public Guid Id
        {
            get { return _data.Id; }
            set { _data.Id = value; }
        }

        public string Username
        {
            get { return _data.Username; }
            set { _data.Username = value; }
        }

        public string FirstName
        {
            get { return _data.FirstName; }
            set { _data.FirstName = value; }
        }

        public string LastName
        {
            get { return _data.LastName; }
            set { _data.LastName = value; }
        }

        public string Language
        {
            get { return _data.Language; }
            set { _data.Language = value; }
        }

        public UserStatus Status
        {
            get { return _data.Status; }
            set { _data.Status = value; }
        }
        public string PasswordEncrypted
        {
            get { return _data.PasswordEncrypted; }
            set { _data.PasswordEncrypted = value; }
        }

        public string Description
        {
            get { return _data.Description; }
            set { _data.Description = value; }
        }

        public ICollection<UserLicenseeId> Licensees
        {
            get { return _data.Licensees; }
            set { _data.Licensees = value; }
        }

        public Data.Role Role
        {
            get { return _data.Role; }
            set { _data.Role = value; }
        }

        public ICollection<BrandId> AllowedBrands
        {
            get { return _data.AllowedBrands; }
        }

        public ICollection<CurrencyCode> Currencies
        {
            get { return _data.Currencies; }
        }

        public ICollection<BrandFilterSelection> BrandFilterSelections
        {
            get { return _data.BrandFilterSelections; }
        }

        public ICollection<LicenseeFilterSelection> LicenseeFilterSelections
        {
            get { return _data.LicenseeFilterSelections; }
        }

        public bool IsSuperAdmin
        {
            get { return Role.IsSuperAdmin; }
        }

        
    }
}
