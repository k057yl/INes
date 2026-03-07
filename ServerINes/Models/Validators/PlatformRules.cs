using FluentValidation;
using INest.Models.DTOs.Platform;

namespace INest.Models.Validators
{
    public class PlatformRules : AbstractValidator<PlatformDto>
    {
        public PlatformRules()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
        }
    }
}
