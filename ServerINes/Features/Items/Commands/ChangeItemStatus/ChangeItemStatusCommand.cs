using INest.Data.Enums;
using MediatR;

namespace INest.Features.Items.Commands.ChangeItemStatus
{
    public record ChangeItemStatusCommand(Guid UserId, Guid ItemId, ItemStatus NewStatus) : IRequest<bool>;
}