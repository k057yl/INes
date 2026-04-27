using INest.Models.DTOs.Item;
using MediatR;

namespace INest.Services.Features.Items.Commands.UpdateItemPartial
{
    public record UpdateItemPartialCommand(
        Guid UserId,
        Guid ItemId,
        UpdateItemPartialDto Dto,
        List<IFormFile>? Photos
    ) : IRequest<bool>;
}