using INest.Models.DTOs.Sale;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace INest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SalesController : ControllerBase
    {
        private readonly ISalesService _salesService;

        public SalesController(ISalesService salesService) => _salesService = salesService;

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> SellItem([FromBody] SellItemRequestDto request)
        {
            try
            {
                return Ok(await _salesService.SellItemAsync(GetUserId(), request));
            }
            catch (Exception ex) { return BadRequest(ex.Message); }
        }

        [HttpGet]
        public async Task<IActionResult> GetSales() => Ok(await _salesService.GetSalesAsync(GetUserId()));

        [HttpDelete("cancel/{itemId}")]
        public async Task<IActionResult> CancelSale(Guid itemId)
        {
            var result = await _salesService.CancelSaleAsync(GetUserId(), itemId);
            return result ? Ok() : NotFound();
        }

        [HttpDelete("smart-delete/{saleId}")]
        public async Task<IActionResult> SmartDelete(Guid saleId, [FromQuery] bool keepHistory = true)
        {
            var userId = GetUserId();

            if (keepHistory)
            {
                var result = await _salesService.SmartDeleteAsync(userId, saleId);
                return result ? NoContent() : NotFound();
            }
            else
            {
                var result = await _salesService.DeleteSaleRecordAsync(userId, saleId);
                return result ? NoContent() : NotFound();
            }
        }
    }
}