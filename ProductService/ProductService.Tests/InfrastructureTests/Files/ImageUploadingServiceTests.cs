using Moq;
using ProductService.Application.Abstractions.Files;
using ProductService.Application.Abstractions.Files.ImageUploading;
using ProductService.Infrastructure.Files;
using Xunit;

namespace ProductService.Tests.InfrastructureTests.Files;

public class ImageUploadingServiceTests
{
    [Fact]
        public async Task UploadImageAsync_ReturnsListOfUploadedFiles()
        {
            
            var mockImageUploader = new Mock<IImageUploader>();
            var service = new ImageUploadingService(mockImageUploader.Object);
            
            var mockFiles = new Mock<IFormFileAdapter>[]
            {
                new Mock<IFormFileAdapter>(),
                new Mock<IFormFileAdapter>()
            };
            
            var files = mockFiles.Select(m => m.Object).ToList();
            var webRootPath = "/test/wwwroot";
            var cancellationToken = CancellationToken.None;
            var dirToUpload = "uploads";
            
            
            mockImageUploader.Setup(x => x.UploadImageAsync(
                    mockFiles[0].Object, webRootPath, cancellationToken, dirToUpload))
                .ReturnsAsync("uploads/file1.jpg");
            
            mockImageUploader.Setup(x => x.UploadImageAsync(
                    mockFiles[1].Object, webRootPath, cancellationToken, dirToUpload))
                .ReturnsAsync("uploads/file2.jpg");

            
            var result = await service.UploadImageAsync(files, webRootPath, cancellationToken, dirToUpload);

            
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Contains("uploads/file1.jpg", result);
            Assert.Contains("uploads/file2.jpg", result);
            
            
            mockImageUploader.Verify(x => x.UploadImageAsync(
                It.IsAny<IFormFileAdapter>(), webRootPath, cancellationToken, dirToUpload), Times.Exactly(2));
        }
        
        [Fact]
        public async Task UploadImageAsync_ReturnsEmptyList_WhenFilesIsNull()
        {
            
            var mockImageUploader = new Mock<IImageUploader>();
            var service = new ImageUploadingService(mockImageUploader.Object);
            
            
            var result = await service.UploadImageAsync(null, "/test/wwwroot", CancellationToken.None);

            
            Assert.NotNull(result);
            Assert.Empty(result);
            
            
            mockImageUploader.Verify(x => x.UploadImageAsync(
                It.IsAny<IFormFileAdapter>(), It.IsAny<string>(), It.IsAny<CancellationToken>(), It.IsAny<string>()), Times.Never);
        }
}