using INest.Constants;
using INest.Exceptions;
using INest.Models.DTOs.Sale;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static INest.Constants.LocalizationConstants;

namespace INest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _salesService;

        public SalesController(ISalesService salesService) => _salesService = salesService;

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new AppException(LocalizationConstants.AUTH.ERRORS.TOKEN_MISSING, 401);
            }
            return Guid.Parse(userIdClaim);
        }

        [HttpPost]
        public async Task<IActionResult> SellItem([FromBody] SellItemRequestDto request)
        {
            var result = await _salesService.SellItemAsync(GetUserId(), request);
            return Ok(new { data = result, message = SALES.SUCCESS.SELL });
        }

        [HttpGet]
        public async Task<IActionResult> GetSales() => Ok(await _salesService.GetSalesAsync(GetUserId()));

        [HttpDelete("cancel/{itemId}")]
        public async Task<IActionResult> CancelSale(Guid itemId)
        {
            await _salesService.CancelSaleAsync(GetUserId(), itemId);
            return Ok(new { message = SALES.SUCCESS.CANCEL });
        }

        [HttpDelete("smart-delete/{saleId}")]
        public async Task<IActionResult> SmartDelete(Guid saleId, [FromQuery] bool keepHistory = true)
        {
            var userId = GetUserId();
            if (keepHistory)
                await _salesService.SmartDeleteAsync(userId, saleId);
            else
                await _salesService.DeleteSaleRecordAsync(userId, saleId);

            return Ok(new { message = SALES.SUCCESS.DELETE });
        }
    }
}