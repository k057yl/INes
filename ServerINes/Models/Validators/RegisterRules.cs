using FluentValidation;
using INest.Models.DTOs.Auth;
using Microsoft.Extensions.Localization;

namespace INest.Models.Validators
{
    public class RegisterRules : AbstractValidator<RegisterDto>
    {
        public RegisterRules(IStringLocalizer<RegisterRules> T)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(x => T["EmailRequired"])
                .EmailAddress().WithMessage(x => T["InvalidEmailFormat"]);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(x => T["PasswordRequired"])
                .MinimumLength(6).WithMessage(x => T["PasswordTooShort"]);

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(x => T["UsernameRequired"])
                .MinimumLength(3).WithMessage(x => T["UsernameTooShort"]);
        }
    }
}
