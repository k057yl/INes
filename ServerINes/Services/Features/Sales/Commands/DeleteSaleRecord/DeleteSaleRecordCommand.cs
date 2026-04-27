using MediatR;

namespace INest.Services.Features.Sales.Commands.DeleteSaleRecord
{
    public record DeleteSaleRecordCommand(Guid UserId, Guid SaleId) : IRequest<bool>;
}
