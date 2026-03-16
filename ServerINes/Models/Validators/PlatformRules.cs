using FluentValidation;
using INest.Models.DTOs.Platform;
using INest.Constants;

namespace INest.Models.Validators
{
    public class PlatformRules : AbstractValidator<PlatformDto>
    {
        public PlatformRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage(LocalizationConstants.ERRORS.REQUIRED_FIELD)
                .MaximumLength(50)
                .WithMessage(LocalizationConstants.ERRORS.MAX_LENGTH_50);
        }
    }
}