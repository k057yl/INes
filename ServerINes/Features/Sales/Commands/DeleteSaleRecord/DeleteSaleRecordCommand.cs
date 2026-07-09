using MediatR;

namespace INest.Features.Sales.Commands.DeleteSaleRecord
{
    public record DeleteSaleRecordCommand(Guid UserId, Guid SaleId) : IRequest<bool>;
}
