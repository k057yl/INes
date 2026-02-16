using INest.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace INest.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PlatformsController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PlatformsController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            return Ok(await _context.Platforms.Where(p => p.UserId == userId).ToListAsync());
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] string name)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var platform = new Platform { Id = Guid.NewGuid(), Name = name, UserId = userId };
            _context.Platforms.Add(platform);
            await _context.SaveChangesAsync();
            return Ok(platform);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var platform = await _context.Platforms.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

            if (platform == null) return NotFound();

            _context.Platforms.Remove(platform);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
