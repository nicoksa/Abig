// ImageConversionService.cs
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using Abig2025.Services.Interfaces;

namespace Abig2025.Services
{
    public class ImageConversionService : IImageConversionService
    {
        public async Task<Stream> ConvertToWebpAsync(IFormFile imageFile, int quality = 75)
        {
            using var imageStream = imageFile.OpenReadStream();
            using var image = await Image.LoadAsync(imageStream);

            var webpStream = new MemoryStream();
            var webpEncoder = new WebpEncoder
            {
                Quality = quality,
                Method = WebpEncodingMethod.Level4 // Balance entre velocidad y compresión
            };

            await image.SaveAsync(webpStream, webpEncoder);
            webpStream.Position = 0;

            return webpStream;
        }

        public async Task<string> ConvertAndSaveAsync(IFormFile imageFile, string savePath, int quality = 75)
        {
            using var webpStream = await ConvertToWebpAsync(imageFile, quality);

            using var fileStream = new FileStream(savePath, FileMode.Create);
            await webpStream.CopyToAsync(fileStream);

            return savePath;
        }
    }
}
