using MediatR;

namespace INest.Features.Items.Commands.CancelSale
{
    public record CancelSaleCommand(Guid UserId, Guid ItemId) : IRequest<bool>;
}