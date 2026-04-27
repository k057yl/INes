using MediatR;

namespace INest.Services.Features.Items.Commands.DeleteItemsBatch
{
    public record DeleteItemsBatchCommand(Guid UserId, List<Guid> ItemIds) : IRequest<bool>;
}