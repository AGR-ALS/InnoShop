namespace ProductService.Application.Abstractions.Files;

public interface IFormFileAdapter
{
    string FileName { get; }
    Task CopyToAsync(Stream target, CancellationToken cancellationToken);
}