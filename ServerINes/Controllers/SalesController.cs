using INest.Models.DTOs.Sale;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly AppDbContext _context;

        public SalesController(ISalesService salesService, IItemService itemService, AppDbContext context)
        {
            _salesService = salesService;
            _itemService = itemService;
            _context = context;
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

        [HttpDelete("smart-delete/{saleId}")]
        public async Task<IActionResult> SmartDelete(Guid saleId, [FromQuery] bool keepHistory = true)
        {
            var userId = GetCurrentUserId();

            var sale = await _context.Sales.FirstOrDefaultAsync(s => s.Id == saleId);
            if (sale == null) return NotFound();

            if (keepHistory)
            {
                if (sale.ItemId.HasValue)
                    await _itemService.DeleteAsync(userId, sale.ItemId.Value);
            }
            else
            {
                var itemId = sale.ItemId;
                await _salesService.DeleteSaleRecordAsync(userId, saleId);

                if (itemId.HasValue)
                    await _itemService.DeleteAsync(userId, itemId.Value);
            }

            return NoContent();
        }
    }
}