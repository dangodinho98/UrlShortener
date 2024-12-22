namespace UrlShortener.Infrastructure.Migrations;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using UrlShortener.Infrastructure.Data;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IHost app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        try
        {
            var dbContext = services.GetRequiredService<UrlShortenerDbContext>();
            dbContext.Database.Migrate();
        }
        catch (Exception ex)
        {
            // Log the error or rethrow it
            throw new ApplicationException("An error occurred while applying migrations.", ex);
        }
    }
}