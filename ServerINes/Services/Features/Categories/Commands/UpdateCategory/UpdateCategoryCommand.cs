using INest.Data.Entities.Core;
using INest.Models.DTOs.Category;
using MediatR;

namespace INest.Services.Features.Categories.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(Guid UserId, Guid CategoryId, CreateCategoryDto Dto) : IRequest<Category>;
}
