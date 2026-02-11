using Microsoft.Extensions.Configuration;
using ProductService.Application.Abstractions.Files;
using ProductService.Application.Abstractions.Files.ImageUploading;

namespace ProductService.Infrastructure.Files;

public class ImageUploader : IImageUploader
{

    public async Task<string?> UploadImageAsync(IFormFileAdapter? file, string webRootPath, CancellationToken cancellationToken, string dirToUpload = "uploads")
    {
        string? filePath = null;
        string? fileName = null;
        if (file != null)
        {
            try
            {
                var uploadsDir = Path.Combine(webRootPath, dirToUpload);
                Directory.CreateDirectory(uploadsDir);
            
                fileName =  Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                filePath = Path.Combine(uploadsDir, fileName);
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException("No web root folder was configured(such as wwwroot)", ex);
            }
            using (var stream =  new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }
            filePath = $"{dirToUpload}/{fileName}";
        }
        return filePath;
    }
}