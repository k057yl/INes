using INest.Constants;
using INest.Exceptions;
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
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var cat = await _service.CreateAsync(GetUserId(), dto);
            return CreatedAtAction(nameof(GetAll), new { }, cat);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryDto dto)
        {
            return Ok(await _service.UpdateAsync(GetUserId(), id, dto));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid? targetCategoryId = null)
        {
            await _service.DeleteAsync(GetUserId(), id, targetCategoryId);
            return NoContent();
        }
    }
}