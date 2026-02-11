using Microsoft.AspNetCore.Http;
using ProductService.Application.Abstractions.Files;

namespace ProductService.Infrastructure.Files;

public class FormFileAdapter : IFormFileAdapter
{
    private readonly IFormFile _formFile;

    public FormFileAdapter(IFormFile formFile)
    {
        _formFile = formFile;
    }

    public string FileName => _formFile.FileName;

    public Task CopyToAsync(Stream target, CancellationToken cancellationToken)
    {
        return _formFile.CopyToAsync(target, cancellationToken);
    }
}