namespace ProductService.Application.Abstractions.Files.ImageUploading;

public interface IImageUploadingService
{
    Task<List<string>> UploadImageAsync(IEnumerable<IFormFileAdapter>? files, string webRootPath, CancellationToken cancellationToken, string dirToUpload = "uploads");
}