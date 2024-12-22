using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Interfaces.Repositories;

internal class UrlShorteningRepository(UrlShortenerDbContext dbContext) : IUrlShorteningRepository
{
    /// <inheritdoc />
    public async Task<bool> CodeExistsAsync(string code) => await dbContext.ShortenedUrls.AnyAsync(u => u.Code == code);

    /// <inheritdoc />
    public async Task Save(ShortenedUrl? shortenedUrl)
    {
        dbContext.ShortenedUrls.Add(shortenedUrl);
        await dbContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task<ShortenedUrl?> GetByCode(string code)
    {
        return await dbContext.ShortenedUrls.FirstOrDefaultAsync(s=> code.Equals(s!.Code));
    }
}