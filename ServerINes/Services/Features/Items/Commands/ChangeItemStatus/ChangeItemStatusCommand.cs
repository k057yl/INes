using INest.Models.Enums;
using MediatR;

namespace INest.Services.Features.Items.Commands.ChangeItemStatus
{
    public record ChangeItemStatusCommand(Guid UserId, Guid ItemId, ItemStatus NewStatus) : IRequest<bool>;
}