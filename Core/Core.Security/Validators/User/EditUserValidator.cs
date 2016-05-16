using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AFT.RegoV2.Core.Security.ApplicationServices.Data.Users;

namespace AFT.RegoV2.Core.Security.Validators
{
    public class EditUserValidator : UserValidatorBase<EditUserData>
    {
        public EditUserValidator()
        {
            ValidateUsername();

            ValidateFirstName();

            ValidateLastName();

            ValidateAssignedLicensees();

            ValidateAllowedBrands();

            ValidateCurrencies();
        }
    }
}
