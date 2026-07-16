using INest.Features.Telegram.Commands.GenerateTelegramToken;
using INest.Features.Telegram.Commands.UnlinkTelegram;
using INest.Features.Telegram.Dtos;
using INest.Features.Telegram.Queries.GetTelegramStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace INest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/telegram")]
    public class TelegramController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TelegramController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("status")]
        public async Task<ActionResult<TelegramStatusDto>> GetStatus(CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.Send(new GetTelegramStatusQuery(userId), ct);
            return Ok(result);
        }

        [HttpPost("generate-token")]
        public async Task<ActionResult<TelegramStatusDto>> GenerateToken(CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            var result = await _mediator.Send(new GenerateTelegramTokenCommand(userId), ct);
            return Ok(result);
        }

        [HttpPost("unlink")]
        public async Task<IActionResult> Unlink(CancellationToken ct)
        {
            var userId = GetCurrentUserId();
            var success = await _mediator.Send(new UnlinkTelegramCommand(userId), ct);
            return success ? Ok() : BadRequest();
        }

        private Guid GetCurrentUserId()
        {
            // Вытаскиваешь UserId из контекста авторизации (NameIdentifier или твой кастомный claim)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(userIdStr, out var userId) ? userId : Guid.Empty;
        }
    }
}