using INest.Models.DTOs.Category;
using INest.Models.Entities;
using MediatR;

namespace INest.Services.Features.Categories.Commands.UpdateCategory
{
    public record UpdateCategoryCommand(Guid UserId, Guid CategoryId, CreateCategoryDto Dto) : IRequest<Category>;
}
