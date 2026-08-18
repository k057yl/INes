using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using INest.Constants;
using INest.Infrastructure.Storage;

namespace INest.Infrastructure
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<PhotoService> _logger;
        private const long MaxFileSizeBytes = 10 * 1024 * 1024;

        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };
        private static readonly string[] AllowedReceiptExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".webp" };

        public PhotoService(Cloudinary cloudinary, ILogger<PhotoService> logger)
        {
            _cloudinary = cloudinary;
            _logger = logger;
        }

        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file, Guid? userId = null)
        {
            if (file == null || file.Length == 0) return new ImageUploadResult();

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            {
                _logger.LogWarning("Попытка загрузить недопустимое расширение файла: {FileName} ({ContentType})", file.FileName, file.ContentType);
                return new ImageUploadResult { Error = new Error { Message = LocalizationConstants.ERRORS.IMAGE_PROCESSING_FAILED } };
            }

            if (file.Length > MaxFileSizeBytes)
            {
                _logger.LogWarning("Превышен размер файла: {Size} байт", file.Length);
                return new ImageUploadResult { Error = new Error { Message = LocalizationConstants.ERRORS.FILE_TOO_LARGE } };
            }

            try
            {
                await using var stream = file.OpenReadStream();

                var folderPath = userId.HasValue
                    ? $"INest/users/{userId.Value}"
                    : "INest";

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folderPath,
                    Transformation = new Transformation().Width(1280).Crop("limit").Quality("auto").FetchFormat("auto")
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult.Error != null)
                {
                    _logger.LogError("Cloudinary Upload Error: {Message}", uploadResult.Error.Message);
                }

                return uploadResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при загрузке фото в Cloudinary для файла {FileName}", file.FileName);
                return new ImageUploadResult { Error = new Error { Message = ex.Message } };
            }
        }

        public async Task<ImageUploadResult> AddReceiptAsync(IFormFile file, Guid? userId = null)
        {
            if (file == null || file.Length == 0) return new ImageUploadResult();

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedReceiptExtensions.Contains(extension))
            {
                _logger.LogWarning("Попытка загрузить недопустимый документ чека: {FileName}", file.FileName);
                return new ImageUploadResult { Error = new Error { Message = LocalizationConstants.ERRORS.IMAGE_PROCESSING_FAILED } };
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return new ImageUploadResult { Error = new Error { Message = LocalizationConstants.ERRORS.FILE_TOO_LARGE } };
            }

            var folderPath = userId.HasValue
                ? $"INest/users/{userId.Value}/receipts"
                : "INest/receipts";

            await using var stream = file.OpenReadStream();

            if (extension == ".pdf" || file.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var rawUploadParams = new RawUploadParams
                    {
                        File = new FileDescription(file.FileName, stream),
                        Folder = folderPath,
                        UseFilename = true,
                        UniqueFilename = true
                    };

                    var rawResult = await _cloudinary.UploadAsync(rawUploadParams);

                    if (rawResult.Error != null)
                    {
                        _logger.LogError("Cloudinary Raw Upload Error: {Message}", rawResult.Error.Message);
                        return new ImageUploadResult { Error = rawResult.Error };
                    }

                    return new ImageUploadResult
                    {
                        SecureUrl = rawResult.SecureUrl,
                        PublicId = rawResult.PublicId
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "PDF receipt upload to Cloudinary failed");
                    return new ImageUploadResult { Error = new Error { Message = ex.Message } };
                }
            }

            try
            {
                var imageUploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = folderPath,
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                var imageResult = await _cloudinary.UploadAsync(imageUploadParams);

                if (imageResult.Error != null)
                {
                    _logger.LogError("Cloudinary Image Receipt Upload Error: {Message}", imageResult.Error.Message);
                }

                return imageResult;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Image receipt upload to Cloudinary failed");
                return new ImageUploadResult { Error = new Error { Message = ex.Message } };
            }
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            return await _cloudinary.DestroyAsync(new DeletionParams(publicId));
        }
    }
}