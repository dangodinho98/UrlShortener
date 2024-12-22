using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces.Repositories;

public interface IUrlShorteningRepository
{
    Task<bool> CodeExistsAsync(string code);
    Task Save(ShortenedUrl? shortenedUrl);
    Task<ShortenedUrl?> GetByCode(string code);
}