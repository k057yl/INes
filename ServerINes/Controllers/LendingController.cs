using INest.Exceptions;
using INest.Models.DTOs.Lending;
using INest.Services.Features.Lendings.Commands.LendItem;
using INest.Services.Features.Lendings.Commands.ReturnItem;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static INest.Constants.LocalizationConstants;

namespace INest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LendingController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LendingController(IMediator mediator)
            => _mediator = mediator;

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new AppException(AUTH.ERRORS.TOKEN_MISSING, 401);
            }
            return Guid.Parse(userIdClaim);
        }

        [HttpPost("lend")]
        public async Task<IActionResult> Lend([FromBody] LendItemDto dto)
        {
            var result = await _mediator.Send(new LendItemCommand(GetUserId(), dto));
            return Ok(new { data = result, message = LENDING.SUCCESS.LEND });
        }

        [HttpPost("{itemId}/return")]
        public async Task<IActionResult> Return(Guid itemId, [FromBody] ReturnItemDto dto)
        {
            var result = await _mediator.Send(new ReturnItemCommand(GetUserId(), itemId, dto));
            return Ok(new { data = result, message = LENDING.SUCCESS.RETURN });
        }
    }
}