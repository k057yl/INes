using INest.Exceptions;
using INest.Models.DTOs.Reminder;
using INest.Services.Features.Reminders.Commands.AddReminder;
using INest.Services.Features.Reminders.Commands.CompleteReminder;
using INest.Services.Features.Reminders.Commands.DeleteReminder;
using INest.Services.Features.Reminders.Queries.GetActiveReminders;
using INest.Services.Features.Reminders.Queries.GetItemReminders;
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
    public class RemindersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RemindersController(IMediator mediator)
            => _mediator = mediator;

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new AppException(AUTH.ERRORS.TOKEN_MISSING, 401);
            }
            return Guid.Parse(userIdClaim);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive()
            => Ok(await _mediator.Send(new GetActiveRemindersQuery(GetUserId())));

        [HttpGet("item/{itemId}")]
        public async Task<IActionResult> GetByItem(Guid itemId)
            => Ok(await _mediator.Send(new GetItemRemindersQuery(GetUserId(), itemId)));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateReminderDto dto)
        {
            var reminder = await _mediator.Send(new AddReminderCommand(GetUserId(), dto));
            return Ok(new { data = reminder, message = REMINDERS.SUCCESS.CREATE });
        }

        [HttpPatch("{id}/complete")]
        public async Task<IActionResult> Complete(Guid id)
        {
            await _mediator.Send(new CompleteReminderCommand(GetUserId(), id));
            return Ok(new { message = REMINDERS.SUCCESS.COMPLETE });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteReminderCommand(GetUserId(), id));
            return Ok(new { message = REMINDERS.SUCCESS.DELETE });
        }
    }
}