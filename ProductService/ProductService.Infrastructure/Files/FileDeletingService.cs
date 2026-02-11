using MassTransit.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using ProductService.Application.Abstractions.Files;
using ProductService.Application.Exceptions;
using ProductService.Infrastructure.Files.StaticFiles;

namespace ProductService.Infrastructure.Files;

public class FileDeletingService : IFileDeletingService
{
    private readonly StaticFilesSettings _staticFilesSettings;

    public FileDeletingService(IOptions<StaticFilesSettings> staticFilesSettings)
    {
        _staticFilesSettings = staticFilesSettings.Value;
    }
    public async Task DeleteFilesAsync(List<string> filesToDelete, CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            foreach (var fileToDelete in filesToDelete)
            {
                var filePath = $"{_staticFilesSettings.Path}/{fileToDelete}";
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return;
                }

                throw new NotFoundException("Files were not found");
            }
        }, cancellationToken);
    }
}