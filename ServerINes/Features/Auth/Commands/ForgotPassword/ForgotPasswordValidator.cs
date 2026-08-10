using FluentValidation;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.ForgotPassword
{
    public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordCommand>
    {
        public ForgotPasswordValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD)
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible)
                .WithMessage(ERRORS.INVALID_EMAIL_FORMAT);
        }
    }
}