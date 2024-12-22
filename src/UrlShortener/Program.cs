using UrlShortener.Application;
using UrlShortener.Application.Interfaces.Services;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.Models;
using UrlShortener.Infrastructure;
using UrlShortener.Infrastructure.Migrations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddOpenApi();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.ApplyMigrations();
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
        options.RoutePrefix = string.Empty; // Serve Swagger UI at the root
    });
}

app.MapPost("api/shorten", async (ShortenUrlRequest request, IUrlShorteningService service, HttpContext httpContext) =>
{
    if (Uri.TryCreate(request.Url, UriKind.Absolute, out _))
    {
        return Results.BadRequest("Invalid url format");
    }

    var code = await service.GenerateUniqueCode();

    var shortenedUrl = new ShortenedUrl()
    {
        Id = Guid.NewGuid(),
        LongUrl = request.Url,
        Code = code,
        ShortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/{code}",
        CreatedAtUtc = DateTime.UtcNow
    };

    await service.Save(shortenedUrl);

    return Results.Ok(shortenedUrl.ShortUrl);
});

app.MapGet("api/{code}", async (string code, IUrlShorteningService service) =>
{
    var shortenedUrl = await service.GetByCode(code);
    return shortenedUrl is null ? Results.NotFound() : Results.Redirect(shortenedUrl.LongUrl);
});

app.UseHttpsRedirection();
app.Run();

