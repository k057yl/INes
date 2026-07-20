using MediatR;

namespace INest.Features.Items.Commands.DeleteArchivedItem
{
    public record DeleteArchivedItemCommand(Guid UserId, Guid ItemId) : IRequest;
}
