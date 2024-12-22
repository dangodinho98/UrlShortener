namespace UrlShortener.UnitTests.Services;

using Microsoft.Extensions.Caching.Distributed;
using Moq;
using System.Text.Json;
using System.Threading.Tasks;
using UrlShortener.Application.Interfaces.Repositories;
using UrlShortener.Application.Services;
using UrlShortener.Domain.Contants;
using UrlShortener.Domain.Entities;

public class UrlShorteningServiceTests
{
    private readonly Mock<IUrlShorteningRepository> _repositoryMock;
    private readonly Mock<IDistributedCache> _cacheMock;
    private readonly UrlShorteningService _service;

    public UrlShorteningServiceTests()
    {
        _repositoryMock = new Mock<IUrlShorteningRepository>();
        _cacheMock = new Mock<IDistributedCache>();
        _service = new UrlShorteningService(_repositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task GenerateUniqueCode_ShouldReturnUniqueCode()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync(It.IsAny<string>(), default))
            .ReturnsAsync((byte[])null);

        _repositoryMock.Setup(r => r.CodeExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var code = await _service.GenerateUniqueCode();

        // Assert
        Assert.NotNull(code);
        Assert.Equal(ShorteningConstants.NumberOfCharsInShortLink, code.Length);

        _cacheMock.Verify(c => c.SetAsync(It.IsAny<string>(), It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(), default), Times.Once);
    }

    [Fact]
    public async Task GenerateUniqueCode_ShouldRetryOnConflict()
    {
        // Arrange
        _cacheMock.SetupSequence(c => c.GetAsync(It.IsAny<string>(), default))
                  .ReturnsAsync(new byte[1]) // Simulate cache conflict
                  .ReturnsAsync((byte[])null); // No cache conflict on retry

        _repositoryMock.Setup(r => r.CodeExistsAsync(It.IsAny<string>()))
                       .ReturnsAsync(false);

        // Act
        var code = await _service.GenerateUniqueCode();

        // Assert
        Assert.NotNull(code);
        _cacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), default), Times.AtLeast(2));
    }

    [Fact]
    public async Task Save_ShouldSaveToRepositoryAndCache()
    {
        // Arrange
        var shortenedUrl = new ShortenedUrl
        {
            Code = "abc123",
            LongUrl = "https://example.com"
        };

        // Act
        await _service.Save(shortenedUrl);

        // Assert
        _repositoryMock.Verify(r => r.Save(shortenedUrl), Times.Once);

        _cacheMock.Verify(c => c.SetAsync(
            $"ShortUrl:{shortenedUrl.Code}",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            default), Times.Once);
    }

    [Fact]
    public async Task GetByCode_ShouldReturnFromCache_WhenCached()
    {
        // Arrange
        var code = "abc123";
        var shortenedUrl = new ShortenedUrl { Code = code, LongUrl = "https://example.com" };
        var cachedValue = JsonSerializer.SerializeToUtf8Bytes(shortenedUrl);

        _cacheMock.Setup(c => c.GetAsync($"ShortUrl:{code}", default))
                  .ReturnsAsync(cachedValue);

        // Act
        var result = await _service.GetByCode(code);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(shortenedUrl.Code, result.Code);
        Assert.Equal(shortenedUrl.LongUrl, result.LongUrl);

        _repositoryMock.Verify(r => r.GetByCode(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task GetByCode_ShouldFallbackToRepository_WhenNotCached()
    {
        // Arrange
        var code = "abc123";
        var shortenedUrl = new ShortenedUrl { Code = code, LongUrl = "https://example.com" };

        _cacheMock.Setup(c => c.GetAsync($"ShortUrl:{code}", default))
                  .ReturnsAsync((byte[])null);

        _repositoryMock.Setup(r => r.GetByCode(code))
                       .ReturnsAsync(shortenedUrl);

        // Act
        var result = await _service.GetByCode(code);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(shortenedUrl.Code, result.Code);
        Assert.Equal(shortenedUrl.LongUrl, result.LongUrl);

        _cacheMock.Verify(c => c.SetAsync(
            $"ShortUrl:{code}",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            default), Times.Once);
    }
}