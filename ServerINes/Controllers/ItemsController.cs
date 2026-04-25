using INest.Constants;
using INest.Exceptions;
using INest.Models.DTOs.Item;
using INest.Models.Enums;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static INest.Constants.LocalizationConstants;

namespace INest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;
        public ItemsController(IItemService itemService) => _itemService = itemService;

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new AppException(LocalizationConstants.AUTH.ERRORS.TOKEN_MISSING, 401);
            }
            return Guid.Parse(userIdClaim);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateItemDto dto, List<IFormFile> photos)
        {
            var item = await _itemService.CreateItemAsync(GetUserId(), dto, photos);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, new
            {
                data = item,
                message = ITEMS.SUCCESS.CREATE
            });
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
            await _itemService.UpdateFullAsync(GetUserId(), id, dto, photos);
            return Ok(new { message = ITEMS.SUCCESS.UPDATE });
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(Guid id, [FromForm] UpdateItemPartialDto dto, List<IFormFile>? photos)
        {
            var result = await _itemService.UpdatePartialAsync(GetUserId(), id, dto, photos);
            return result ? Ok() : BadRequest();
        }

        [HttpPatch("{id}/move")]
        public async Task<IActionResult> Move(Guid id, [FromBody] MoveItemDto dto)
        {
            await _itemService.MoveItemAsync(GetUserId(), id, dto.TargetLocationId);
            return Ok(new { message = HISTORY.MOVED });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ItemStatus status)
        {
            await _itemService.ChangeStatusAsync(GetUserId(), id, status);
            return Ok();
        }

        [HttpDelete("{id}/sale")]
        public async Task<IActionResult> CancelSale(Guid id)
        {
            await _itemService.CancelSaleAsync(GetUserId(), id);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _itemService.DeleteAsync(GetUserId(), id);
            return Ok(new { message = ITEMS.SUCCESS.DELETE });
        }

        [HttpDelete("batch")]
        public async Task<IActionResult> DeleteBatch([FromBody] List<Guid> ids)
        {
            if (ids == null || !ids.Any()) return BadRequest();
            await _itemService.DeleteBatchAsync(GetUserId(), ids);
            return NoContent();
        }
    }
}