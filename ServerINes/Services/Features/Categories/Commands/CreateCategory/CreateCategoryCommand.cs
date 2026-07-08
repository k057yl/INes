using INest.Data.Entities.Core;
using INest.Models.DTOs.Category;
using MediatR;

namespace INest.Services.Features.Categories.Commands.CreateCategory
{
    public record CreateCategoryCommand(Guid UserId, CreateCategoryDto Dto) : IRequest<Category>;
}
