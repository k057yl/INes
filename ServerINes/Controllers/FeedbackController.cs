using INest.Features.Feedback.Commands.CreateFeedback;
using INest.Features.Feedback.Commands.RateFeedback;
using INest.Features.Feedback.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace INest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly IMediator _mediator;

        public FeedbackController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateFeedbackDto dto)
        {
            var userId = Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var id = await _mediator.Send(new CreateFeedbackCommand(userId, dto.Type, dto.Message));
            return Ok(new { Id = id });
        }

        [HttpPost("{id:guid}/rate")]
        public async Task<IActionResult> Rate([FromRoute] Guid id, [FromBody] RateFeedbackDto dto)
        {
            await _mediator.Send(new RateFeedbackCommand(id, dto.Rating, dto.MissingFeatures));
            return NoContent();
        }
    }
}
