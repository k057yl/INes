using FluentValidation;

namespace INest.Services.Features.Categories.Commands.DeleteCategory
{
    public class DeleteCategoryValidator : AbstractValidator<DeleteCategoryCommand>
    {
        public DeleteCategoryValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.CategoryId).NotEmpty();
        }
    }
}
