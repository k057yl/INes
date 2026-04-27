using MediatR;

namespace INest.Services.Features.Items.Commands.CancelSale
{
    public record CancelSaleCommand(Guid UserId, Guid ItemId) : IRequest<bool>;
}