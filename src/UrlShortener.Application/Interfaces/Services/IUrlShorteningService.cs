using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces.Services;

public interface IUrlShorteningService
{
    Task<string> GenerateUniqueCode();
    Task Save(ShortenedUrl? shortenedUrl);
    Task<ShortenedUrl?> GetByCode(string code);
}