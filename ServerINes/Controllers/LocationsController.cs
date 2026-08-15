using INest.Exceptions;
using INest.Features.Locations.Commands.CreateLocation;
using INest.Features.Locations.Commands.DeleteLocation;
using INest.Features.Locations.Commands.MoveLocation;
using INest.Features.Locations.Commands.RenameLocation;
using INest.Features.Locations.Commands.ReorderLocations;
using INest.Features.Locations.DTOs;
using INest.Features.Locations.Queries.GetLocationChildren;
using INest.Features.Locations.Queries.GetLocationHeader;
using INest.Features.Locations.Queries.GetLocationItems;
using INest.Features.Locations.Queries.GetLocations;
using INest.Features.Locations.Queries.GetLocationTree;
using INest.Infrastructure.QrCode;
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
    public class LocationsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IQrCodeService _qrCodeService;
        private readonly IConfiguration _configuration;

        public LocationsController(IMediator mediator, IQrCodeService qrCodeService, IConfiguration configuration)
        {
            _mediator = mediator;
            _qrCodeService = qrCodeService;
            _configuration = configuration;
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

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _mediator.Send(new GetLocationsQuery(GetUserId())));

        [HttpGet("tree")]
        public async Task<IActionResult> GetTree() => Ok(await _mediator.Send(new GetLocationTreeQuery(GetUserId())));


        [HttpGet("{id}/header")]
        public async Task<IActionResult> GetHeader(Guid id)
        {
            var header = await _mediator.Send(new GetLocationHeaderQuery(GetUserId(), id));
            if (header == null) throw new AppException(LOCATIONS.ERRORS.NOT_FOUND, 404);
            return Ok(header);
        }

        [HttpGet("{id}/items")]
        public async Task<IActionResult> GetItems(Guid id)
        {
            var items = await _mediator.Send(new GetLocationItemsQuery(GetUserId(), id));
            return Ok(items);
        }

        [HttpGet("{id}/children")]
        public async Task<IActionResult> GetChildren(Guid id)
        {
            var children = await _mediator.Send(new GetLocationChildrenQuery(GetUserId(), id));
            return Ok(children);
        }

        [HttpGet("{id}/qr")]
        public async Task<IActionResult> GetQrCode(Guid id)
        {
            var loc = await _mediator.Send(new GetLocationHeaderQuery(GetUserId(), id));
            if (loc == null) throw new AppException(LOCATIONS.ERRORS.NOT_FOUND, 404);

            var frontendUrl = _configuration["Frontend:Url"];
            if (string.IsNullOrWhiteSpace(frontendUrl))
            {
                throw new AppException(SYSTEM.CONFIG_ERROR, 500);
            }

            var locationUrl = string.Format(SYSTEM.LOCATION_QR_PATH, frontendUrl, id);
            var qrCodeBytes = _qrCodeService.GeneratePngCode(locationUrl);

            return File(qrCodeBytes, "image/png");
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLocationDto dto)
        {
            var location = await _mediator.Send(new CreateLocationCommand(GetUserId(), dto));
            return Ok(new { data = location, message = LOCATIONS.SUCCESS.CREATE });
        }

        [HttpPatch("{id}/move")]
        public async Task<IActionResult> Move(Guid id, [FromBody] MoveLocationDto dto)
        {
            await _mediator.Send(new MoveLocationCommand(GetUserId(), id, dto.NewParentId));
            return Ok(new { message = LOCATIONS.SUCCESS.MOVE });
        }

        [HttpPut("reorder")]
        public async Task<IActionResult> Reorder([FromBody] ReorderLocationsDto dto)
        {
            await _mediator.Send(new ReorderLocationsCommand(GetUserId(), dto.ParentId, dto.OrderedIds));
            return Ok();
        }

        [HttpPatch("{id}/rename")]
        public async Task<IActionResult> Rename(Guid id, [FromBody] RenameLocationDto dto)
        {
            await _mediator.Send(new RenameLocationCommand(GetUserId(), id, dto.Name));
            return Ok(new { message = LOCATIONS.SUCCESS.RENAME });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _mediator.Send(new DeleteLocationCommand(GetUserId(), id));
            return Ok(new { message = LOCATIONS.SUCCESS.DELETE });
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteLocation(Guid id, [FromQuery] Guid? targetLocationId, CancellationToken ct)
        {
            var userId = GetUserId();
            await _mediator.Send(new DeleteLocationCommand(id, userId, targetLocationId), ct);
            return Ok(new { message = LOCATIONS.SUCCESS.DELETE });
        }
    }
}