using INest.Models.DTOs.Sale;

namespace INest.Services.Interfaces
{
    public interface ISalesService
    {
        Task<SaleResponseDto> SellItemAsync(Guid userId, SellItemRequestDto request);
        Task<List<SaleResponseDto>> GetSalesAsync(Guid userId);
        Task<bool> CancelSaleAsync(Guid userId, Guid itemId);
        Task<bool> DeleteSaleRecordAsync(Guid userId, Guid saleId);
        Task<bool> SmartDeleteAsync(Guid userId, Guid saleId);
    }
}
