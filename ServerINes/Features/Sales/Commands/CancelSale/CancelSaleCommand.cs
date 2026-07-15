using MediatR;

namespace INest.Features.Sales.Commands.CancelSale
{
    public record CancelSaleCommand(Guid UserId, Guid ItemId, Guid LocationId) : IRequest<bool>;
}