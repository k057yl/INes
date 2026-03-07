using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using INest.Models.Entities;
using INest.Services.Interfaces;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using ISSize = SixLabors.ImageSharp.Size;

namespace INest.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly Cloudinary _cloudinary;
        private const int TargetWidth = 320;
        private const long MaxFileSizeBytes = 512 * 1024;

        public PhotoService(IOptions<CloudinarySettings> config)
        {
            var acc = new Account(config.Value.CloudName, config.Value.ApiKey, config.Value.ApiSecret);
            _cloudinary = new Cloudinary(acc);
        }

        public async Task<ImageUploadResult> AddPhotoAsync(IFormFile file)
        {
            var uploadResult = new ImageUploadResult();

            if (file == null || file.Length == 0) return uploadResult;

            if (file.Length > MaxFileSizeBytes * 20)
            {
                return new ImageUploadResult { Error = new Error { Message = "Файл слишком велик для обработки" } };
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

                    await image.SaveAsJpegAsync(outStream, new SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder
                    {
                        Quality = 75
                    });
                }

                outStream.Position = 0;

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, outStream),
                    Transformation = new Transformation().Quality("auto").FetchFormat("auto")
                };

                uploadResult = await _cloudinary.UploadAsync(uploadParams);
            }
            catch (Exception ex)
            {
                return new ImageUploadResult { Error = new Error { Message = $"Ошибка обработки: {ex.Message}" } };
            }

            return uploadResult;
        }

        public async Task<DeletionResult> DeletePhotoAsync(string publicId)
        {
            var deleteParams = new DeletionParams(publicId);
            return await _cloudinary.DestroyAsync(deleteParams);
        }
    }
}