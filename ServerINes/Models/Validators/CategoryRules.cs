using FluentValidation;
using INest.Models.DTOs.Category;

namespace INest.Models.Validators
{
    public class CategoryRules : AbstractValidator<CreateCategoryDto>
    {
        public CategoryRules()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("ERRORS.REQUIRED_FIELD")
                .MaximumLength(50).WithMessage("ERRORS.MAX_LENGTH_50");

            RuleFor(x => x.Color)
                .Matches("^#(?:[0-9a-fA-F]{3}){1,2}$")
                .WithMessage("ERRORS.INVALID_COLOR_HEX");
        }
    }
}
