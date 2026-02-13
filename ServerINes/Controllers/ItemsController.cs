using INest.Models.DTOs.Item;
using INest.Models.Enums;
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

        public ItemsController(IItemService itemService)
        {
            _itemService = itemService;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("UserId missing"));

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromForm] CreateItemDto dto,
            IFormFile? photo)
        {
            var item = await _itemService.CreateItemAsync(GetUserId(), dto, photo);
            return Ok(item);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var items = await _itemService.GetUserItemsAsync(GetUserId());
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var item = await _itemService.GetItemAsync(GetUserId(), id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateItemDto dto)
        {
            var updated = await _itemService.UpdateItemAsync(GetUserId(), id, dto);
            if (!updated) return NotFound();
            return Ok();
        }

        [HttpPatch("{id}/move")]
        public async Task<IActionResult> Move(Guid id, [FromBody] MoveItemDto dto)
        {
            var moved = await _itemService.MoveItemAsync(GetUserId(), id, dto.TargetLocationId);
            if (!moved) return NotFound();
            return Ok();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ItemStatus newStatus)
        {
            var changed = await _itemService.ChangeStatusAsync(GetUserId(), id, newStatus);
            if (!changed) return NotFound();
            return Ok();
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> History(Guid id)
        {
            var history = await _itemService.GetItemHistoryAsync(GetUserId(), id);
            return Ok(history);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _itemService.DeleteItemAsync(GetUserId(), id);
            if (!deleted) return NotFound();
            return Ok();
        }
    }
}