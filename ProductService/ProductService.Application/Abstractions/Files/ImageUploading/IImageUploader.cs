namespace ProductService.Application.Abstractions.Files.ImageUploading;

public interface IImageUploader
{
    Task<string?> UploadImageAsync(IFormFileAdapter? file, string webRootPath, CancellationToken cancellationToken, string dirToUpload = "uploads");
}