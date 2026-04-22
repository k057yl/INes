using INest.Constants;
using INest.Exceptions;
using INest.Models.DTOs.Platform;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace INest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlatformsController : ControllerBase
    {
        private readonly IPlatformService _service;
        public PlatformsController(IPlatformService service) => _service = service;

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new AppException(LocalizationConstants.AUTH.ERRORS.TOKEN_MISSING, 401);
            }
            return Guid.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync(GetUserId()));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PlatformDto dto)
        {
            var platform = await _service.CreateAsync(GetUserId(), dto);
            return Ok(platform);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] PlatformDto dto)
        {
            var updated = await _service.UpdateAsync(GetUserId(), id, dto);
            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(GetUserId(), id);
            return NoContent();
        }
    }
}