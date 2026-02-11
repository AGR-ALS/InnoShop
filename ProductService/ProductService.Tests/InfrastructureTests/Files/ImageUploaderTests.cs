using Moq;
using ProductService.Application.Abstractions.Files;
using ProductService.Infrastructure.Files;
using Xunit;

namespace ProductService.Tests.InfrastructureTests.Files;

public class ImageUploaderTests
{
    private readonly string _testDirectory;
        private readonly string _webRootPath;

        public ImageUploaderTests()
        {
            
            _testDirectory = Path.Combine(Path.GetTempPath(), "ImageUploaderTests");
            _webRootPath = Path.Combine(_testDirectory, "wwwroot");
            Directory.CreateDirectory(_webRootPath);
        }

        public void Dispose()
        {
            
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }
        }

        [Fact]
        public async Task UploadImageAsync_ReturnsFilePath_WhenFileExists()
        {
            
            var imageUploader = new ImageUploader();
            var mockFile = new Mock<IFormFileAdapter>();
            
            mockFile.Setup(f => f.FileName).Returns("test-image.jpg");
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            
            var result = await imageUploader.UploadImageAsync(
                mockFile.Object, 
                _webRootPath, 
                CancellationToken.None, 
                "uploads");

            
            Assert.NotNull(result);
            Assert.StartsWith("uploads/", result);
            Assert.EndsWith(".jpg", result);
            Assert.Contains(".jpg", result);
            
            
            var fullPath = Path.Combine(_webRootPath, result);
            Assert.True(File.Exists(fullPath));
            
            
            var uploadsDir = Path.Combine(_webRootPath, "uploads");
            Assert.True(Directory.Exists(uploadsDir));
        }

        [Fact]
        public async Task UploadImageAsync_ReturnsNull_WhenFileIsNull()
        {
            
            var imageUploader = new ImageUploader();

            
            var result = await imageUploader.UploadImageAsync(
                null, 
                _webRootPath, 
                CancellationToken.None, 
                "uploads");

            
            Assert.Null(result);
        }
}