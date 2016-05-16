using AFT.RegoV2.AdminWebsite.Validators;
using FluentValidation.Attributes;

namespace AFT.RegoV2.AdminWebsite.ViewModels
{
    [Validator(typeof(LoginValidator))]
    public class LoginViewModel
    {
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public bool RememberMe { get; set; }
    }
}