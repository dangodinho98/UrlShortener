namespace UrlShortener.Application.Services;

using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using UrlShortener.Application.Interfaces.Repositories;
using UrlShortener.Application.Interfaces.Services;
using UrlShortener.Domain.Contants;
using UrlShortener.Domain.Entities;

internal class UrlShorteningService(IUrlShorteningRepository repository, IDistributedCache cache) : IUrlShorteningService
{
    private const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghjiklmnopqrstuvwxyz0123456789";
    private readonly Random _random = new Random();

    public async Task<string> GenerateUniqueCode()
    {
        var code = new char[ShorteningConstants.NumberOfCharsInShortLink];

        while (true)
        {
            var generatedCode = GenerateRandomCode();

            // Check Redis cache
            if (await IsCodeCachedAsync(generatedCode)) continue;

            // Check database
            if (await repository.CodeExistsAsync(generatedCode)) continue;

            // Cache the code to prevent duplicates
            await CacheCodeAsync(generatedCode);

            return generatedCode;
        }
    }

    /// <inheritdoc />
    public async Task Save(ShortenedUrl? shortenedUrl)
    {
        if (shortenedUrl == null) throw new ArgumentNullException(nameof(shortenedUrl)); 
        
        await repository.Save(shortenedUrl);

        var cacheKey = $"ShortUrl:{shortenedUrl.Code}";
        var cacheValue = JsonSerializer.Serialize(shortenedUrl);

        await cache.SetStringAsync(cacheKey, cacheValue, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
        });
    }

    /// <inheritdoc />
    public async Task<ShortenedUrl?> GetByCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code cannot be null or empty.", nameof(code));

        var cacheKey = GetCacheKeyForUrl(code);

        // Check cache first
        var cachedUrl = await cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedUrl))
        {
            return JsonSerializer.Deserialize<ShortenedUrl>(cachedUrl);
        }

        // Fallback to database
        var shortenedUrl = await repository.GetByCode(code);
        if (shortenedUrl != null)
        {
            // Cache the result
            await cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(shortenedUrl), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
            });
        }

        return shortenedUrl;
    }

    private string GenerateRandomCode()
    {
        var code = new char[ShorteningConstants.NumberOfCharsInShortLink];
        for (var i = 0; i < ShorteningConstants.NumberOfCharsInShortLink; i++)
        {
            var randomIndex = _random.Next(Alphabet.Length);
            code[i] = Alphabet[randomIndex];
        }
        return new string(code);
    }

    /// <summary>
    /// Checks if a code is already cached
    /// </summary>
    private async Task<bool> IsCodeCachedAsync(string code)
    {
        var cacheKey = GetCacheKeyForCode(code);
        return !string.IsNullOrEmpty(await cache.GetStringAsync(cacheKey));
    }

    /// <summary>
    /// Caches a generated code
    /// </summary>
    private async Task CacheCodeAsync(string code)
    {
        var cacheKey = GetCacheKeyForCode(code);
        await cache.SetStringAsync(cacheKey, "exists", new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        });
    }

    /// <summary>
    /// Constructs cache key for short code
    /// </summary>
    private static string GetCacheKeyForCode(string code) => $"ShortCode:{code}";

    /// <summary>
    /// Constructs cache key for shortened URL
    /// </summary>
    private static string GetCacheKeyForUrl(string code) => $"ShortUrl:{code}";
}