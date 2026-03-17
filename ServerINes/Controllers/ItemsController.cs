using INest.Models.DTOs.Item;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace INest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;
        public ItemsController(IItemService itemService) => _itemService = itemService;

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateItemDto dto, List<IFormFile> photos)
        {
            var item = await _itemService.CreateItemAsync(GetUserId(), dto, photos);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _itemService.GetUserItemsAsync(GetUserId()));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _itemService.GetItemAsync(GetUserId(), id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UpdateItemDto dto, List<IFormFile>? photos)
        {
            await _itemService.UpdateItemAsync(GetUserId(), id, dto, photos);
            return Ok();
        }

        [HttpPatch("{id}/move")]
        public async Task<IActionResult> Move(Guid id, [FromBody] MoveItemDto dto)
        {
            await _itemService.MoveItemAsync(GetUserId(), id, dto.TargetLocationId);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _itemService.DeleteAsync(GetUserId(), id);
            return NoContent();
        }
    }
}