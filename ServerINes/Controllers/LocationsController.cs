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
                // 1. Ищем клейм (пробуем стандартный и короткий "sub")
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

                if (string.IsNullOrEmpty(userIdStr))
                    return BadRequest(new { error = "В токене отсутствует ID пользователя" });

                // 2. Безопасно парсим в Guid
                if (!Guid.TryParse(userIdStr, out var userId))
                    return BadRequest(new { error = $"ID в токене ({userIdStr}) не является валидным GUID" });

                // 3. Вызываем сервис
                var locations = await _locationService.GetUserLocationsAsync(userId);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                // Если упадет база или что-то еще - мы увидим текст ошибки на фронте
                return StatusCode(500, new { error = ex.Message, stack = ex.StackTrace });
            }
        }
    }
}
