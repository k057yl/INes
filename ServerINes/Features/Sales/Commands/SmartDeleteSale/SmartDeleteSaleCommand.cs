using MediatR;

namespace INest.Features.Sales.Commands.SmartDeleteSale
{
    public record SmartDeleteSaleCommand(Guid UserId, Guid SaleId) : IRequest<bool>;
}
