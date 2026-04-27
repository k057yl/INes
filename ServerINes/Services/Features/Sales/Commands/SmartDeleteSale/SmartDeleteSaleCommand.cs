using MediatR;

namespace INest.Services.Features.Sales.Commands.SmartDeleteSale
{
    public record SmartDeleteSaleCommand(Guid UserId, Guid SaleId) : IRequest<bool>;
}
