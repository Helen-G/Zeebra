using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Users;
using AFT.RegoV2.Core.Services.Security;
using FluentValidation;

namespace AFT.RegoV2.Core.Security.Validators
{
    public class AddUserValidator : UserValidatorBase<AddUserData>
    {
        public AddUserValidator()
        {
            ValidateUsername();

            ValidateFirstName();

            ValidateLastName();

            ValidatePassword();

            ValidateAssignedLicensees();

            ValidateAllowedBrands();

            ValidateCurrencies();
        }
    }
}
