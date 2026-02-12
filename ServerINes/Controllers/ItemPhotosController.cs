using INest.Models.DTOs.ItemPhoto;
using INest.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace INest.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ItemPhotosController : ControllerBase
    {
        private readonly IItemPhotoService _photoService;

        public ItemPhotosController(IItemPhotoService photoService)
        {
            _photoService = photoService;
        }

        private Guid GetUserId() =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new Exception("UserId missing"));

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] UploadItemPhotoDto dto)
        {
            var photo = await _photoService.UploadPhotoAsync(GetUserId(), dto.ItemId, dto.File, dto.IsMain);
            return Ok(photo);
        }

        [HttpGet("{itemId}")]
        public async Task<IActionResult> GetPhotos(Guid itemId)
        {
            var photos = await _photoService.GetPhotosAsync(GetUserId(), itemId);
            return Ok(photos);
        }

        [HttpDelete("{photoId}")]
        public async Task<IActionResult> Delete(Guid photoId)
        {
            var deleted = await _photoService.DeletePhotoAsync(GetUserId(), photoId);
            if (!deleted) return NotFound();
            return Ok();
        }
    }
}
