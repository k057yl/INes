using INest.Data.Entities.Core;
using INest.Features.Categories.DTOs;
using MediatR;

namespace INest.Features.Categories.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(Guid UserId, Guid CategoryId, CreateCategoryDto Dto) : IRequest<Category>;
}
