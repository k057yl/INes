using FluentValidation;
using INest.Models.DTOs.Auth;
using static INest.Constants.LocalizationConstants;

namespace INest.Models.Validators
{
    public class ConfirmRegisterRules : AbstractValidator<ConfirmRegisterDto>
    {
        public ConfirmRegisterRules()
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
