using Microsoft.Extensions.Configuration;
using Moq;
using ProductService.Application.Exceptions;
using ProductService.Infrastructure.Files;
using Xunit;

namespace ProductService.Tests.InfrastructureTests.Files;

public class FileDeletingServiceTests
{
    private readonly string _testDirectory;
    private readonly string _testFilePath;

    public FileDeletingServiceTests()
    {
        
        _testDirectory = Path.Combine(Path.GetTempPath(), "FileDeletingServiceTests");
        Directory.CreateDirectory(_testDirectory);

        
        _testFilePath = Path.Combine(_testDirectory, "testfile.txt");
        File.WriteAllText(_testFilePath, "Test content");
    }

    public void Dispose()
    {
        
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    [Fact]
    public async Task DeleteFilesAsync_DeletesExistingFile()
    {
        
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["StaticFiles:Path"])
            .Returns(_testDirectory);

        var service = new FileDeletingService(configurationMock.Object);
        var filesToDelete = new List<string> { "testfile.txt" };

        
        Assert.True(File.Exists(_testFilePath));

        
        await service.DeleteFilesAsync(filesToDelete, CancellationToken.None);

        
        Assert.False(File.Exists(_testFilePath));
    }

    [Fact]
    public async Task DeleteFilesAsync_ThrowsNotFoundException_WhenFileDoesNotExist()
    {
        
        var configurationMock = new Mock<IConfiguration>();
        configurationMock.Setup(x => x["StaticFiles:Path"])
            .Returns(_testDirectory);

        var service = new FileDeletingService(configurationMock.Object);
        var filesToDelete = new List<string> { "nonexistentfile.txt" };

        
        await Assert.ThrowsAsync<NotFoundException>(() =>
            service.DeleteFilesAsync(filesToDelete, CancellationToken.None));
    }
}