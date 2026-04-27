using INest.Models.DTOs.Item;
using INest.Models.Entities;
using MediatR;

namespace INest.Services.Features.Items.Commands.CreateItem
{
    public record CreateItemCommand(Guid UserId, CreateItemDto Dto, List<IFormFile> Photos) : IRequest<Item>;
}
