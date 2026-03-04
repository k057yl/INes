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
        private readonly IItemService _itemService;

        public SalesController(ISalesService salesService, IItemService itemService)
        {
            _salesService = salesService;
            _itemService = itemService;
        }

        [HttpPost]
        public async Task<ActionResult<SaleResponseDto>> SellItem([FromBody] SellItemRequestDto request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _salesService.SellItemAsync(userId, request);
                return Ok(result);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<ActionResult<List<SaleResponseDto>>> GetSales()
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _salesService.GetSalesAsync(userId);
                return Ok(result);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }

        private Guid GetCurrentUserId()
        {
            var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)
                          ?? User.FindFirst("sub")
                          ?? User.FindFirst("id");

            if (idClaim != null && Guid.TryParse(idClaim.Value, out Guid userId))
            {
                return userId;
            }

            throw new UnauthorizedAccessException("User ID is missing in token.");
        }

        [HttpDelete("{itemId}")]
        public async Task<IActionResult> CancelSale(Guid itemId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _salesService.CancelSaleAsync(userId, itemId);

                if (!result)
                    return NotFound("Sale record not found for this item.");

                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error during sale cancellation");
            }
        }

        [HttpDelete("{saleId}/permanent")]
        public async Task<IActionResult> DeletePermanent(Guid saleId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var sales = await _salesService.GetSalesAsync(userId);
                var sale = sales.FirstOrDefault(s => s.SaleId == saleId);

                if (sale == null) return NotFound("Продажа не найдена");

                var result = await _itemService.PermanentDeleteAsync(userId, sale.ItemId);

                if (!result) return BadRequest("Не удалось удалить объект");

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
        }
    }
}