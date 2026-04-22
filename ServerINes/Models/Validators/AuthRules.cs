using FluentValidation;
using INest.Models.DTOs.Auth;
using static INest.Constants.LocalizationConstants;

namespace INest.Models.Validators
{
    public class LoginRules : AbstractValidator<LoginDto>
    {
        public LoginRules()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD)
                .EmailAddress().WithMessage(ERRORS.INVALID_EMAIL_FORMAT);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD);
        }
    }

    public class ForgotPasswordRules : AbstractValidator<ForgotPasswordDto>
    {
        public ForgotPasswordRules()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD)
                .EmailAddress().WithMessage(ERRORS.INVALID_EMAIL_FORMAT);
        }
    }
}