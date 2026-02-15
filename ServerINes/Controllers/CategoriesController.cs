using INest.Models.DTOs.Category;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace INest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _service;
        public CategoriesController(ICategoryService service) => _service = service;

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("UserId missing"));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var cat = await _service.CreateAsync(GetUserId(), dto);
            return Ok(cat);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var cats = await _service.GetAllAsync(GetUserId());
            return Ok(cats);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryDto dto)
        {
            var updated = await _service.UpdateAsync(GetUserId(), id, dto);

            if (updated == null)
                return NotFound(new { message = "Категория не найдена или доступ запрещен" });

            return Ok(updated);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _service.DeleteAsync(GetUserId(), id);

            if (!success)
                return NotFound(new { message = "Категория не найдена или доступ запрещен" });

            return NoContent();
        }
    }
}
