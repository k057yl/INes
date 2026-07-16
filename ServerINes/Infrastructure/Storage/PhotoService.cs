using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using INest.Constants;
using INest.Infrastructure.Storage;
using SkiaSharp;

namespace INest.Infrastructure
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<PhotoService> _logger;
        private const int TargetWidth = 320;
        private const long MaxFileSizeBytes = 512 * 1024;

        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/webp", "image/gif" };

        public PhotoService(Cloudinary cloudinary, ILogger<PhotoService> logger)
        {
            _cloudinary = cloudinary;
            _logger = logger;
        }

        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file, Guid? userId = null)
        {
            if (file == null || file.Length == 0) return new ImageUploadResult();

            if (!AllowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            {
                _logger.LogWarning("Попытка загрузить неверный формат файла: {Type}", file.ContentType);
                return new ImageUploadResult { Error = new Error { Message = LocalizationConstants.ERRORS.IMAGE_PROCESSING_FAILED } };
            }

            if (file.Length > MaxFileSizeBytes * 20)
            {
                return new ImageUploadResult { Error = new Error { Message = LocalizationConstants.ERRORS.FILE_TOO_LARGE } };
            }

            using var outStream = new MemoryStream();
            try
            {
                using (var inputStream = file.OpenReadStream())
                using (var originalBitmap = SKBitmap.Decode(inputStream))
                {
                    if (originalBitmap == null)
                        throw new Exception(LocalizationConstants.ERRORS.IMAGE_PROCESSING_FAILED);

                    int targetHeight = (int)(originalBitmap.Height * ((float)TargetWidth / originalBitmap.Width));

                    using (var resizedBitmap = new SKBitmap(TargetWidth, targetHeight))
                    {
                        var samplingOptions = new SKSamplingOptions(SKFilterMode.Linear);
                        originalBitmap.ScalePixels(resizedBitmap, samplingOptions);

                        using (var image = SKImage.FromBitmap(resizedBitmap))
                        using (var data = image.Encode(SKEncodedImageFormat.Jpeg, 75))
                        {
                            data.SaveTo(outStream);
                        }
                    }
                }

                outStream.Position = 0;

                var folderPath = userId.HasValue
                    ? $"INest/users/{userId.Value}"
                    : "INest";

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, outStream),
                    Folder = folderPath,
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto"),
                };

                return await _cloudinary.UploadAsync(uploadParams);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Photo processing via SkiaSharp failed");
                return new ImageUploadResult { Error = new Error { Message = LocalizationConstants.ERRORS.IMAGE_PROCESSING_FAILED } };
            }
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            return await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        }
    }
}