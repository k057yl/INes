using INest.Features.Items.DTOs;
using MediatR;

namespace INest.Features.Items.Commands.UpdateItemPartial
{
    public record UpdateItemPartialCommand(
        Guid UserId,
        Guid ItemId,
        UpdateItemPartialDto Dto,
        List<IFormFile>? Photos,
        IFormFile? ReceiptFile = null
    ) : IRequest<bool>;
}