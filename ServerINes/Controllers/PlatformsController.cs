using INest.Exceptions;
using INest.Models.DTOs.Platform;
using INest.Services.Features.Platforms.Commands.CreatePlatform;
using INest.Services.Features.Platforms.Commands.DeletePlatform;
using INest.Services.Features.Platforms.Commands.UpdatePlatform;
using INest.Services.Features.Platforms.Queries.GetPlatforms;
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
    public class PlatformsController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PlatformsController(IMediator mediator) => _mediator = mediator;

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new AppException(AUTH.ERRORS.TOKEN_MISSING, 401);
            }
            return Guid.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _mediator.Send(new GetPlatformsQuery(GetUserId())));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PlatformDto dto)
        {
            var platform = await _mediator.Send(new CreatePlatformCommand(GetUserId(), dto));
            return Ok(new { data = platform, message = PLATFORMS.SUCCESS.CREATE });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PlatformDto dto)
        {
            var updated = await _mediator.Send(new UpdatePlatformCommand(GetUserId(), id, dto));
            return Ok(new { data = updated, message = PLATFORMS.SUCCESS.UPDATE });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeletePlatformCommand(GetUserId(), id));
            return Ok(new { message = PLATFORMS.SUCCESS.DELETE });
        }
    }
}