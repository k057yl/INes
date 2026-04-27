using INest.Exceptions;
using INest.Models.DTOs.Sale;
using INest.Services.Features.Items.Commands.CancelSale;
using INest.Services.Features.Sales.Commands.DeleteSaleRecord;
using INest.Services.Features.Sales.Commands.SellItem;
using INest.Services.Features.Sales.Commands.SmartDeleteSale;
using INest.Services.Features.Sales.Queries.GetSales;
using MediatR;
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
        private readonly IMediator _mediator;

        public SalesController(IMediator mediator) => _mediator = mediator;

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new AppException(AUTH.ERRORS.TOKEN_MISSING, 401);
            }
            return Guid.Parse(userIdClaim);
        }

        [HttpPost]
        public async Task<IActionResult> SellItem([FromBody] SellItemRequestDto request)
        {
            var command = new SellItemCommand(GetUserId(), request);
            var result = await _mediator.Send(command);
            return Ok(new { data = result, message = SALES.SUCCESS.SELL });
        }

        [HttpGet]
        public async Task<IActionResult> GetSales()
        {
            var query = new GetSalesQuery(GetUserId());
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpDelete("cancel/{itemId}")]
        public async Task<IActionResult> CancelSale(Guid itemId)
        {
            var command = new CancelSaleCommand(GetUserId(), itemId);
            await _mediator.Send(command);
            return Ok(new { message = SALES.SUCCESS.CANCEL });
        }

        [HttpDelete("smart-delete/{saleId}")]
        public async Task<IActionResult> SmartDelete(Guid saleId, [FromQuery] bool keepHistory = true)
        {
            var userId = GetUserId();

            if (keepHistory)
            {
                await _mediator.Send(new SmartDeleteSaleCommand(userId, saleId));
            }
            else
            {
                await _mediator.Send(new DeleteSaleRecordCommand(userId, saleId));
            }

            return Ok(new { message = SALES.SUCCESS.DELETE });
        }
    }
}