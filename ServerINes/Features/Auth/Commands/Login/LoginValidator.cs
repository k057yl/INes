using FluentValidation;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.Login
{
    public class LoginValidator : AbstractValidator<LoginCommand>
    {
        public LoginValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD)
                .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible)
                .WithMessage(ERRORS.INVALID_EMAIL_FORMAT);

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD);
        }
    }
}