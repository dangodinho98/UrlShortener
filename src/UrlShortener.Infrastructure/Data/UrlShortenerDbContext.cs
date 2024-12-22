namespace UrlShortener.Infrastructure.Data;

using Microsoft.EntityFrameworkCore;
using UrlShortener.Domain.Contants;
using UrlShortener.Domain.Entities;

public class UrlShortenerDbContext(DbContextOptions<UrlShortenerDbContext> options) : DbContext(options)
{
    public DbSet<ShortenedUrl?> ShortenedUrls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<ShortenedUrl>(builder =>
        {
            builder.Property(s => s.Code).HasMaxLength(ShorteningConstants.NumberOfCharsInShortLink);
            builder.Property(s => s.Code).IsUnicode();
        });
    }
}