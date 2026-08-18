using INest.Data.Enums;
using INest.Exceptions;
using INest.Features.Dashboard.Queries.GetDashboardStats;
using INest.Features.Items.Commands.ChangeItemStatus;
using INest.Features.Items.Commands.CreateItem;
using INest.Features.Items.Commands.DeleteArchivedItem;
using INest.Features.Items.Commands.DeleteArchivedItemsBatch;
using INest.Features.Items.Commands.DeleteItem;
using INest.Features.Items.Commands.DeleteItemsBatch;
using INest.Features.Items.Commands.MoveItem;
using INest.Features.Items.Commands.UpdateItemFull;
using INest.Features.Items.Commands.UpdateItemPartial;
using INest.Features.Items.DTOs;
using INest.Features.Items.Queries.GetItemById;
using INest.Features.Items.Queries.GetItemHistory;
using INest.Features.Items.Queries.GetItems;
using MediatR;
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
        private readonly IMediator _mediator;

        public ItemsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new AppException(AUTH.ERRORS.TOKEN_MISSING, 401);
            }
            return Guid.Parse(userIdClaim);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateItemDto dto, [FromForm] List<IFormFile> photos)
        {
            var command = new CreateItemCommand(GetUserId(), dto, photos);
            var item = await _mediator.Send(command);
            return Ok(item);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ItemFilterDto filters)
        {
            var query = new GetItemsQuery(GetUserId(), filters);
            var items = await _mediator.Send(query);
            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetItemByIdQuery(GetUserId(), id);
            var item = await _mediator.Send(query);
            return Ok(item);
        }

        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateFull(Guid itemId, [FromForm] UpdateItemFullDto dto)
        {
            var command = new UpdateItemFullCommand(
                GetUserId(),
                itemId,
                dto,
                dto.Photos,
                dto.ReceiptFile,
                dto.MainPhotoName
            );
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpPatch("{id}")]
        public async Task<IActionResult> UpdatePartial(Guid id, [FromForm] UpdateItemPartialDto dto)
        {
            var command = new UpdateItemPartialCommand(
                GetUserId(),
                id,
                dto,
                dto.Photos,
                dto.ReceiptFile,
                dto.MainPhotoName
            );
            var result = await _mediator.Send(command);
            return result ? Ok() : BadRequest();
        }

        [HttpPatch("{id}/move")]
        public async Task<IActionResult> Move(Guid id, [FromBody] MoveItemDto dto)
        {
            var command = new MoveItemCommand(GetUserId(), id, dto.TargetLocationId);
            await _mediator.Send(command);
            return Ok(new { message = HISTORY.MOVED });
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ItemStatus status)
        {
            var command = new ChangeItemStatusCommand(GetUserId(), id, status);
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPost("{id}/archive")]
        public async Task<IActionResult> Archive(Guid id)
        {
            var command = new DeleteItemCommand(GetUserId(), id);
            await _mediator.Send(command);
            return Ok(new { message = ITEMS.SUCCESS.DELETE });
        }

        [HttpDelete("batch")]
        public async Task<IActionResult> DeleteBatch([FromBody] List<Guid> ids)
        {
            var command = new DeleteItemsBatchCommand(GetUserId(), ids);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteArchivedItemCommand(GetUserId(), id);
            await _mediator.Send(command);
            return Ok(new { message = ITEMS.SUCCESS.DELETE });
        }

        [HttpDelete("archived/batch")]
        public async Task<IActionResult> DeleteArchivedBatch([FromBody] List<Guid> ids)
        {
            var command = new DeleteArchivedItemsBatchCommand(GetUserId(), ids);
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpGet("{id}/history")]
        public async Task<IActionResult> GetHistory(Guid id)
        {
            var query = new GetItemHistoryQuery(GetUserId(), id);
            var history = await _mediator.Send(query);
            return Ok(history);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var query = new GetDashboardStatsQuery(GetUserId());
            var stats = await _mediator.Send(query);
            return Ok(stats);
        }
    }
}