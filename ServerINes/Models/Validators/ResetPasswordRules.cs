using FluentValidation;
using INest.Constants;
using INest.Models.DTOs.Auth;

namespace INest.Models.Validators
{
    public class ResetPasswordRules : AbstractValidator<ResetPasswordDto>
    {
        public ResetPasswordRules()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD)
                .EmailAddress().WithMessage(LocalizationConstants.ERRORS.INVALID_EMAIL_FORMAT);

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD)
                .MinimumLength(6).WithMessage(LocalizationConstants.ERRORS.PWD_MIN_LENGTH)
                .Matches(@"^[\u0000-\u007F]+$").WithMessage(LocalizationConstants.ERRORS.PWD_LATIN)
                .Matches(@"[A-Z]").WithMessage(LocalizationConstants.ERRORS.PWD_UPPER)
                .Matches(@"[0-9]").WithMessage(LocalizationConstants.ERRORS.PWD_DIGIT)
                .Matches(@"[^a-zA-Z0-9]").WithMessage(LocalizationConstants.ERRORS.PWD_SPEC);

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage(LocalizationConstants.AUTH.TOKEN_MISSING);
        }
    }
}