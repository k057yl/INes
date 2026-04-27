using INest.Models.DTOs.Item;
using MediatR;

namespace INest.Services.Features.Items.Commands.UpdateItemFull
{
    public record UpdateItemFullCommand(
        Guid UserId,
        Guid ItemId,
        UpdateItemFullDto Dto,
        List<IFormFile>? Photos
    ) : IRequest<bool>;
}