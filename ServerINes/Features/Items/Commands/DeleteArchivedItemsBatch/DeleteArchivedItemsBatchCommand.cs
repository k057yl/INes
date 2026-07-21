using MediatR;

namespace INest.Features.Items.Commands.DeleteArchivedItemsBatch
{
    public record DeleteArchivedItemsBatchCommand(Guid UserId, List<Guid> ItemIds) : IRequest<bool>;
}
