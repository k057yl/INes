using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using INest.Constants;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using ISSize = SixLabors.ImageSharp.Size;

namespace INest.Services.Infrastructure
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<PhotoService> _logger;
        private const int TargetWidth = 320;
        private const long MaxFileSizeBytes = 512 * 1024;

        public PhotoService(IOptions<CloudinarySettings> config, ILogger<PhotoService> logger)
        {
            var acc = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
            _cloudinary = new Cloudinary(acc);
            _logger = logger;
        }

        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return new ImageUploadResult();

            if (file.Length > MaxFileSizeBytes * 20)
            {
                return new ImageUploadResult { Error = new Error { Message = LocalizationConstants.ERRORS.FILE_TOO_LARGE } };
            }

            using var outStream = new MemoryStream();
            try
            {
                using (var image = await Image.LoadAsync(file.OpenReadStream()))
                {
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Size = new ISSize(TargetWidth, 0),
                        Mode = ResizeMode.Max
                    }));

                    await image.SaveAsJpegAsync(outStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder { Quality = 75 });
                }

                outStream.Position = 0;
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, outStream),
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                return await _cloudinary.UploadAsync(uploadParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Photo processing failed");
                return new ImageUploadResult { Error = new Error { Message = LocalizationConstants.ERRORS.IMAGE_PROCESSING_FAILED } };
            }
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            return await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        }
    }
}