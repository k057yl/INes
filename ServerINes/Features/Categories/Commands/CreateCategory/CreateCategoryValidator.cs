using FluentValidation;

namespace INest.Features.Categories.Commands.CreateCategory
{
    public class CreateCategoryValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Dto.Name).NotEmpty();
        }
    }
}
