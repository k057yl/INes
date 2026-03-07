using FluentValidation;
using INest.Models.DTOs.Category;

namespace INest.Models.Validators
{
    public class CategoryRules : AbstractValidator<CreateCategoryDto>
    {
        public CategoryRules()
        {
            RuleFor(x => x.Name).NotEmpty().MaximumLength(50);
            RuleFor(x => x.Color).Matches("^#(?:[0-9a-fA-F]{3}){1,2}$")
                .WithMessage("Некорректный формат цвета (HEX)");
        }
    }
}
