using INest.Data.Entities.Core;
using INest.Features.Categories.DTOs;
using MediatR;

namespace INest.Features.Categories.Commands.CreateCategory
{
    public record CreateCategoryCommand(Guid UserId, CreateCategoryDto Dto) : IRequest<Category>;
}
