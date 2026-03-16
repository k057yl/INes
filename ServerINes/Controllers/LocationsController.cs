using INest.Constants;
using INest.Exceptions;
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
        public LocationsController(ILocationService locationService) => _locationService = locationService;

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new AppException("UNAUTHORIZED", 401));

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _locationService.GetUserLocationsAsync(GetUserId()));

        [HttpGet("tree")]
        public async Task<IActionResult> GetTree() => Ok(await _locationService.GetTreeAsync(GetUserId()));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var loc = await _locationService.GetLocationByIdAsync(GetUserId(), id);
            if (loc == null) throw new AppException(LocalizationConstants.LOCATIONS.NOT_FOUND, 404);
            return Ok(loc);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLocationDto dto)
        {
            var location = await _locationService.CreateLocationAsync(GetUserId(), dto);
            return CreatedAtAction(nameof(GetById), new { id = location.Id }, location);
        }

        [HttpPatch("{id}/move")]
        public async Task<IActionResult> Move(Guid id, [FromBody] MoveLocationDto dto)
        {
            await _locationService.MoveLocationAsync(GetUserId(), id, dto.NewParentId);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _locationService.DeleteLocationAsync(GetUserId(), id);
            return NoContent();
        }
    }
}
