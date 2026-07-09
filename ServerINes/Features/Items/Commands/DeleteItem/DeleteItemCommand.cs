using MediatR;

namespace INest.Features.Items.Commands.DeleteItem
{
    public record DeleteItemCommand(Guid UserId, Guid ItemId) : IRequest<bool>;
}