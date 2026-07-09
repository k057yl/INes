using FluentValidation;
using static INest.Constants.LocalizationConstants;

namespace INest.Features.Auth.Commands.ConfirmRegister
{
    public class ConfirmRegisterValidator : AbstractValidator<ConfirmRegisterCommand>
    {
        public ConfirmRegisterValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD)
                .EmailAddress().WithMessage(ERRORS.INVALID_EMAIL_FORMAT);

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD)
                .Length(6).WithMessage(AUTH.ERRORS.INVALID_OR_EXPIRED_CODE);
        }
    }
}
