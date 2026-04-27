using INest.Models.DTOs.Sale;
using MediatR;

namespace INest.Services.Features.Sales.Commands.SellItem
{
    public record SellItemCommand(Guid UserId, SellItemRequestDto Dto) : IRequest<SaleResponseDto>;
}
