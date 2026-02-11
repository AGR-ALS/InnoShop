using ProductService.Application.Abstractions.Files;
using ProductService.Application.Abstractions.Files.ImageUploading;

namespace ProductService.Infrastructure.Files;

public class ImageUploadingService : IImageUploadingService
{
    private readonly IImageUploader _imageUploader;

    public ImageUploadingService(IImageUploader imageUploader)
    {
        _imageUploader = imageUploader;
    }
    
    public async Task<List<string>> UploadImageAsync(IEnumerable<IFormFileAdapter>? files,string webRootPath,  CancellationToken cancellationToken, string dirToUpload = "uploads")
    {
        var uploadedFiles = new List<string>();
        if (files == null)
        {
            return uploadedFiles;
        }
        foreach (var file in files)
        {
            var uploadedFile = await _imageUploader.UploadImageAsync(file, webRootPath, cancellationToken, dirToUpload);
            if (uploadedFile != null)
            {
                uploadedFiles.Add(uploadedFile);
            }
        }
        return uploadedFiles;
    }
}