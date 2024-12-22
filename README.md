# URL Shortener

A lightweight URL Shortener application built using **.NET 9**, **C#**, and **Redis** for caching. This project includes an API for generating and retrieving shortened URLs, backed by a SQL database for persistence and Redis for improved performance.

## Features
- Generate unique short URLs for long links.
- Retrieve original URLs using the short code.
- In-memory caching with Redis to optimize performance.
- Database persistence with EF Core.
- OpenAPI (Swagger) integration for API documentation.

---

## Technology Stack
- **.NET 9**: Core application framework.
- **Entity Framework Core**: ORM for database interactions.
- **Redis**: Distributed caching system.
- **xUnit**: Unit testing framework.
- **Moq**: Mocking library for unit tests.
- **Swashbuckle**: Swagger/OpenAPI documentation.

---

## Getting Started
### Prerequisites
- **.NET SDK**: Version 9 or later.
- **Redis**: Installed locally or available via a cloud provider.
- **SQL Server**: Installed locally or accessible from your environment.

### Installation Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/dangodinho98/UrlShortener.git
   cd UrlShortener
   ```
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Update `appsettings.json`:
   - Set the `DefaultConnection` for SQL Server.
   - Set the `Redis` connection string.

   Example `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=UrlShortenerDb;Trusted_Connection=True;",
       "Redis": "localhost:6379"
     },
     "AllowedHosts": "*"
   }
   ```
4. Apply migrations to the database:
   ```bash
   dotnet ef database update --project src/UrlShortener.Infrastructure --startup-project src/UrlShortener.Api
   ```
5. Run the application:
   ```bash
   dotnet run --project src/UrlShortener.Api
   ```
6. Access Swagger UI for API testing:
   ```
   http://localhost:<port>/swagger
   ```

---

## Project Structure
```
UrlShortener/
├── src/
│   ├── UrlShortener.Api/             # API project
│   ├── UrlShortener.Application/     # Application services and interfaces
│   ├── UrlShortener.Domain/          # Domain entities and constants
│   ├── UrlShortener.Infrastructure/  # Database and caching implementations
├── tests/
│   ├── UrlShortener.Tests/           # Unit tests
```

---

## Key Components

### Database Integration
- **DbContext**: `UrlShortenerDbContext` handles database operations.
- Migrations are managed using EF Core.

### Caching
- **Redis**: Used to cache short URLs and short codes for fast retrieval.

### API Endpoints
| Method | Endpoint        | Description                  |
|--------|-----------------|------------------------------|
| POST   | `/api/shorten`  | Generate a short URL         |
| GET    | `/api/{code}`   | Retrieve the original URL    |

### Example Requests
#### Generate Short URL:
**POST** `/api/shorten`
```json
{
  "url": "https://example.com"
}
```
Response:
```json
{
  "shortUrl": "https://localhost:5001/api/abc123"
}
```

#### Retrieve Original URL:
**GET** `/api/abc123`
Response:
```json
{
  "url": "https://example.com"
}
```

---

## Testing
Run unit tests using xUnit:
```bash
cd tests/UrlShortener.Tests

dotnet test
```

---

