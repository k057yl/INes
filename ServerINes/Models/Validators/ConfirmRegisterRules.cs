using FluentValidation;
using INest.Models.DTOs.Auth;
using Microsoft.Extensions.Localization;

namespace INest.Models.Validators
{
    public class ConfirmRegisterRules : AbstractValidator<ConfirmRegisterDto>
    {
        public ConfirmRegisterRules(IStringLocalizer<ConfirmRegisterRules> T)
        {
            RuleFor(x => x.Email).NotEmpty().EmailAddress();

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage(x => T["CodeRequired"])
                .Length(6).WithMessage(x => T["CodeInvalidLength"]);
        }
    }
}
