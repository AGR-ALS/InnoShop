namespace ProductService.Application.Abstractions.Files;

public interface IFileDeletingService
{ 
    Task DeleteFilesAsync(List<string> filesToDelete, CancellationToken cancellationToken);
}