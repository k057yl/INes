using MediatR;

namespace INest.Services.Features.Items.Commands.MoveItem
{
    public record MoveItemCommand(Guid UserId, Guid ItemId, Guid? TargetLocationId) : IRequest<bool>;
}