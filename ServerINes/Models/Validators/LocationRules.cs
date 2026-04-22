using FluentValidation;
using INest.Models.DTOs.Location;
using static INest.Constants.LocalizationConstants;

namespace INest.Models.Validators
{
    public class LocationRules : AbstractValidator<CreateLocationDto>
    {
        public LocationRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD)
                .MaximumLength(50).WithMessage(ERRORS.MAX_LENGTH_50);
        }
    }

    public class RenameLocationRules : AbstractValidator<RenameLocationDto>
    {
        public RenameLocationRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ERRORS.REQUIRED_FIELD)
                .MaximumLength(50).WithMessage(ERRORS.MAX_LENGTH_50);
        }
    }
}