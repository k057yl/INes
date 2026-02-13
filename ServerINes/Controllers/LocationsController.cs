using INest.Models.DTOs.Location;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace INest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationsController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLocationDto dto)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var location = await _locationService.CreateLocationAsync(userId, dto);
                return Ok(location);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

                if (string.IsNullOrEmpty(userIdStr))
                    return BadRequest(new { error = "В токене отсутствует ID пользователя" });

                if (!Guid.TryParse(userIdStr, out var userId))
                    return BadRequest(new { error = $"ID в токене ({userIdStr}) не является валидным GUID" });

                var locations = await _locationService.GetUserLocationsAsync(userId);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }

        [HttpPatch("{id}/move")]
        public async Task<IActionResult> Move(Guid id, [FromBody] MoveLocationDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _locationService.MoveLocationAsync(userId, id, dto.NewParentId);

            return Ok();
        }

        [HttpPatch("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ReorderLocationsDto dto)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            await _locationService.ReorderLocationsAsync(userId, dto.ParentId, dto.OrderedIds);

            return Ok();
        }

        [HttpGet("tree")]
        public async Task<IActionResult> GetTree()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var tree = await _locationService.GetTreeAsync(userId);

            return Ok(tree);
        }
    }
}
