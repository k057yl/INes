using MediatR;

namespace INest.Services.Features.Items.Commands.DeleteItem
{
    public record DeleteItemCommand(Guid UserId, Guid ItemId) : IRequest<bool>;
}