using INest.Models.DTOs.Lending;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace INest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class LendingController : ControllerBase
    {
        private readonly ILendingService _lendingService;
        public LendingController(ILendingService lendingService) => _lendingService = lendingService;

        [HttpPost("lend")]
        public async Task<IActionResult> Lend([FromBody] LendItemDto dto)
            => Ok(await _lendingService.LendItemAsync(GetUserId(), dto));

        [HttpPost("{itemId}/return")]
        public async Task<IActionResult> Return(Guid itemId, [FromBody] ReturnItemDto dto)
            => Ok(await _lendingService.ReturnItemAsync(GetUserId(), itemId, dto));

        private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
