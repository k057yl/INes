using INest.Data.Entities.Core;
using INest.Features.Items.DTOs;
using MediatR;

namespace INest.Features.Items.Commands.CreateItem
{
    public record CreateItemCommand(Guid UserId, CreateItemDto Dto, List<IFormFile> Photos) : IRequest<Item>;
}
