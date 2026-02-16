using INest.Models.DTOs.Sale;

namespace INest.Services.Interfaces
{
    public interface ISalesService
    {
        Task<SaleResponseDto> SellItemAsync(Guid userId, SellItemRequestDto request);
        Task<List<SaleResponseDto>> GetSalesAsync(Guid userId);
    }
}
