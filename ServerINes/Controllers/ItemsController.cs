using INest.Constants;
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
        public async Task<IActionResult> GetAll([FromQuery] ItemFilterDto filters)
        {
            var items = await _itemService.GetUserItemsAsync(GetUserId(), filters);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _itemService.GetItemAsync(GetUserId(), id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFull(Guid id, [FromForm] UpdateItemFullDto dto, List<IFormFile>? photos)
        {
            try
            {
                var result = await _itemService.UpdateFullAsync(GetUserId(), id, dto, photos);
                return result ? Ok() : BadRequest(LocalizationConstants.SYSTEM.DEFAULT_ERROR);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, LocalizationConstants.SYSTEM.DEFAULT_ERROR);
            }
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial( Guid id,[FromForm] UpdateItemPartialDto dto,List<IFormFile>? photos)
        {
            var result = await _itemService.UpdatePartialAsync(GetUserId(), id, dto, photos);

            return result ? Ok() : BadRequest();
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

        [HttpDelete("bulk")]
        public async Task<IActionResult> BulkDelete([FromBody] List<Guid> ids)
        {
            if (ids == null || !ids.Any())
                return BadRequest(LocalizationConstants.SYSTEM.VALIDATION_FAILED);

            await _itemService.BulkDeleteAsync(GetUserId(), ids);
            return Ok(new { message = LocalizationConstants.ITEMS.DELETE_SUCCESS });
        }
    }
}