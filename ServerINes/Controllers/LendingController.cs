using INest.Constants;
using INest.Exceptions;
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

        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim))
            {
                throw new AppException(LocalizationConstants.AUTH.TOKEN_MISSING, 401);
            }
            return Guid.Parse(userIdClaim);
        }

        [HttpPost("lend")]
        public async Task<IActionResult> Lend([FromBody] LendItemDto dto)
            => Ok(await _lendingService.LendItemAsync(GetUserId(), dto));

        [HttpPost("{itemId}/return")]
        public async Task<IActionResult> Return(Guid itemId, [FromBody] ReturnItemDto dto)
            => Ok(await _lendingService.ReturnItemAsync(GetUserId(), itemId, dto));
    }
}
