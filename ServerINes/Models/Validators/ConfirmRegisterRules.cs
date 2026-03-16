using FluentValidation;
using INest.Constants;
using INest.Models.DTOs.Auth;

namespace INest.Models.Validators
{
    public class ConfirmRegisterRules : AbstractValidator<ConfirmRegisterDto>
    {
        public ConfirmRegisterRules()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD)
                .EmailAddress().WithMessage(LocalizationConstants.ERRORS.INVALID_EMAIL_FORMAT);

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD)
                .Length(6).WithMessage(LocalizationConstants.AUTH.INVALID_OR_EXPIRED_CODE);
        }
    }
}
