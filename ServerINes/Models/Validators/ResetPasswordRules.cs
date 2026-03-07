using FluentValidation;
using INest.Models.DTOs.Auth;
using Microsoft.Extensions.Localization;

namespace INest.Models.Validators
{
    public class ResetPasswordRules : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordRules(IStringLocalizer<ResetPasswordRules> T)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(x => T["EmailRequired"])
                .EmailAddress().WithMessage(x => T["InvalidEmailFormat"]);

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage(x => T["PasswordRequired"])
                .MinimumLength(6).WithMessage(x => T["PasswordTooShort"]);

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage(x => T["TokenMissing"]);
        }
    }
}