using FluentValidation;
using INest.Models.DTOs.Auth;
using Microsoft.Extensions.Localization;

namespace INest.Models.Validators
{
    public class RegisterRules : AbstractValidator<RegisterDto>
    {
        private readonly string _emailRegex = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
        public RegisterRules(IStringLocalizer<SharedResource> T)
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(x => T["EMAIL_REQUIRED"])
                .Matches(_emailRegex).WithMessage(x => T["INVALID_EMAIL_FORMAT"]);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(x => T["PASSWORD_REQUIRED"])
                .MinimumLength(6).WithMessage(x => T["PASSWORD_TOO_SHORT"])
                .Matches(@"[A-Z]").WithMessage(x => T["PASSWORD_REQUIRES_UPPERCASE"])
                .Matches(@"[a-z]").WithMessage(x => T["PASSWORD_REQUIRES_LOWERCASE"])
                .Matches(@"[0-9]").WithMessage(x => T["PASSWORD_REQUIRES_DIGIT"])
                .Matches(@"[^a-zA-Z0-9]").WithMessage(x => T["PASSWORD_REQUIRES_NONALPHANUMERIC"]);

            RuleFor(x => x.Username)
                .NotEmpty().WithMessage(x => T["USERNAME_REQUIRED"])
                .MinimumLength(3).WithMessage(x => T["USERNAME_TOO_SHORT"])
                .Matches(@"^[a-zA-Z0-9]*$").WithMessage(x => T["USERNAME_INVALID_CHARACTERS"]);
        }
    }
}
