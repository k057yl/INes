using MediatR;

namespace INest.Features.Categories.Commands.DeleteCategory
{
    public record DeleteCategoryCommand(Guid UserId, Guid CategoryId, Guid? TargetCategoryId = null) : IRequest<bool>;
}
