using INest.Constants;
using INest.Exceptions;
using INest.Models.DTOs.Reminder;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace INest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RemindersController : ControllerBase
    {
        private readonly IReminderService _reminderService;

        public RemindersController(IReminderService reminderService)
            => _reminderService = reminderService;

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new AppException(LocalizationConstants.AUTH.TOKEN_MISSING, 401);
            }
            return Guid.Parse(userIdClaim);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
            => Ok(await _reminderService.GetActiveRemindersAsync(GetUserId()));

        [HttpGet("item/{itemId}")]
        public async Task<IActionResult> GetByItem(Guid itemId)
            => Ok(await _reminderService.GetItemRemindersAsync(GetUserId(), itemId));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReminderDto dto)
        {
            var reminder = await _reminderService.AddReminderAsync(GetUserId(), dto);
            return CreatedAtAction(nameof(GetByItem), new { itemId = reminder.ItemId }, reminder);
        }

        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            await _reminderService.CompleteReminderAsync(GetUserId(), id);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _reminderService.DeleteReminderAsync(GetUserId(), id);
            return NoContent();
        }
    }
}
