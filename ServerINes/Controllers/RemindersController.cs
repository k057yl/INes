using INest.Exceptions;
using INest.Features.Reminders.DTOs;
using INest.Features.Reminders.Commands.AddReminder;
using INest.Features.Reminders.Commands.CompleteReminder;
using INest.Features.Reminders.Commands.DeleteReminder;
using INest.Features.Reminders.Queries.GetActiveReminders;
using INest.Features.Reminders.Queries.GetItemReminders;
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
            var success = await _mediator.Send(new CompleteReminderCommand(GetUserId(), id));
            if (!success)
            {
                return NotFound(new { message = REMINDERS.ERRORS.NOT_FOUND });
            }
            return Ok(new { message = REMINDERS.SUCCESS.COMPLETE });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var success = await _mediator.Send(new DeleteReminderCommand(GetUserId(), id));
            if (!success)
            {
                return NotFound(new { message = REMINDERS.ERRORS.NOT_FOUND });
            }
            return Ok(new { message = REMINDERS.SUCCESS.DELETE });
        }
    }
}