using INest.Features.Sales.DTOs;
using MediatR;

namespace INest.Features.Sales.Commands.SellItem
{
    public record SellItemCommand(Guid UserId, SellItemRequestDto Dto) : IRequest<SaleResponseDto>;
}
