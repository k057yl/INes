using INest.Exceptions;
using INest.Models.DTOs.Category;
using INest.Services.Features.Categories.Commands.CreateCategory;
using INest.Services.Features.Categories.Commands.DeleteCategory;
using INest.Services.Features.Categories.Commands.UpdateCategory;
using INest.Services.Features.Categories.Queries.GetCategories;
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
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public CategoriesController(IMediator mediator) => _mediator = mediator;

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new AppException(AUTH.ERRORS.TOKEN_MISSING, 401);
            }
            return Guid.Parse(userIdClaim);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll() =>
            Ok(await _mediator.Send(new GetCategoriesQuery(GetUserId())));

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
        {
            var cat = await _mediator.Send(new CreateCategoryCommand(GetUserId(), dto));
            return Ok(new { data = cat, message = CATEGORIES.SUCCESS.CREATE });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateCategoryDto dto)
        {
            var updated = await _mediator.Send(new UpdateCategoryCommand(GetUserId(), id, dto));
            return Ok(new { data = updated, message = CATEGORIES.SUCCESS.UPDATE });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid? targetCategoryId = null)
        {
            await _mediator.Send(new DeleteCategoryCommand(GetUserId(), id, targetCategoryId));
            return Ok(new { message = CATEGORIES.SUCCESS.DELETE });
        }
    }
}