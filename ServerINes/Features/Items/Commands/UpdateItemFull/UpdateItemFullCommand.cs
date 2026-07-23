using INest.Features.Items.DTOs;
using MediatR;

namespace INest.Features.Items.Commands.UpdateItemFull
{
    public record UpdateItemFullCommand(
        Guid UserId,
        Guid ItemId,
        UpdateItemFullDto Dto,
        List<IFormFile>? Photos,
        IFormFile? ReceiptFile = null
    ) : IRequest<bool>;
}