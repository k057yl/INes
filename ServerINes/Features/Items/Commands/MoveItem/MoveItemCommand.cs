using MediatR;

namespace INest.Features.Items.Commands.MoveItem
{
    public record MoveItemCommand(Guid UserId, Guid ItemId, Guid? TargetLocationId) : IRequest<bool>;
}