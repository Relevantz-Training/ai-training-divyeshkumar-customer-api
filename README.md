# Customer API

Customer API is a .NET 8 ASP.NET Core Web API that exposes secured CRUD operations for a basic customer details table. The current implementation uses SQLite through Entity Framework Core so the API has a real lightweight database with minimal operational overhead.

## Solution structure

- `src/CustomerApi`: Main API project.
- `tests/CustomerApi.Tests`: Unit and integration tests.
- `src/CustomerApi/Controllers`: API endpoints.
- `src/CustomerApi/Services`: Business logic and token generation.
- `src/CustomerApi/Data`: EF Core DbContext and SQLite database initialization.
- `src/CustomerApi/Repositories`: Persistence layer backed by SQLite.
- `src/CustomerApi/Contracts`: Request and response DTOs.
- `src/CustomerApi/Middleware`: Exception handling and security headers.

## Implemented features

- CRUD endpoints for customer details.
- .NET 8 Web API with a layered structure.
- JWT bearer authentication.
- Role-based authorization.
- Swagger with bearer-token support.
- Global exception handling using problem details responses.
- Rate limiting and security headers.
- Separate automated test project.
- SQLite-backed customer persistence with automatic seed data.
- Mock users for local authentication testing.

## Mock users for local testing

Use the token endpoint to authenticate with these seeded users:

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@customerapi.local` | `Admin123!` |
| Support | `support@customerapi.local` | `Support123!` |

## Main endpoints

- `POST /api/auth/token`: Returns a JWT for a mock user.
- `GET /api/customers`: Returns all customers from SQLite. Requires `Admin` or `Support` role.
- `GET /api/customers/{id}`: Returns one customer. Requires `Admin` or `Support` role.
- `POST /api/customers`: Creates a customer. Requires `Admin` role.
- `PUT /api/customers/{id}`: Updates a customer. Requires `Admin` role.
- `DELETE /api/customers/{id}`: Deletes a customer. Requires `Admin` role.
- `GET /health`: Health check endpoint.

## Example requests

### Get a token

```http
POST /api/auth/token
Content-Type: application/json

{
  "email": "admin@customerapi.local",
  "password": "Admin123!"
}
```

### Create a customer

```http
POST /api/customers
Authorization: Bearer {token}
Content-Type: application/json

{
  "firstName": "Taylor",
  "lastName": "Morgan",
  "email": "taylor.morgan@example.com",
  "phoneNumber": "+1-555-222-3333",
  "isActive": true
}
```

## Security notes

- JWT configuration is stored under the `Jwt` section in `appsettings.json`.
- SQLite connection string is stored under `ConnectionStrings:CustomerDatabase`.
- Replace the development signing key before any shared or production deployment.
- Keep production secrets out of source control by using user secrets, environment variables, or a secrets manager.
- Swagger is enabled in development by default. Keep production exposure limited unless there is a business need.
- The API returns standardized problem details and avoids exposing internal stack traces.

## Running locally

1. Install the .NET 8 SDK.
2. Restore dependencies with `dotnet restore`.
3. Build the solution with `dotnet build CustomerApi.sln`.
4. Run the API with `dotnet run --project src/CustomerApi/CustomerApi.csproj`.
5. On first run, the API creates the local SQLite file and seeds default customer records automatically.
6. Open Swagger at the local application URL, authenticate through `POST /api/auth/token`, and use the returned bearer token in Swagger.

## Running tests

Run all tests with:

```bash
dotnet test CustomerApi.sln
```

## IIS deployment script

The repository includes a reusable PowerShell deployment script at `scripts/Deploy-CustomerApi.ps1`.

### What the script does

- Restores the API project.
- Runs tests before deployment unless skipped.
- Publishes the API to a local publish folder.
- Optionally copies the published output to an IIS physical path.
- Optionally creates or updates the IIS application pool and website.

### Prerequisites

1. IIS installed on the target server.
2. .NET 8 Hosting Bundle installed on the target server.
3. PowerShell session running as Administrator when using IIS creation or update options.
4. `WebAdministration` module available on the target server.

### Publish only

```powershell
.\scripts\Deploy-CustomerApi.ps1
```

### Publish and deploy to IIS

```powershell
.\scripts\Deploy-CustomerApi.ps1 \
  -CreateOrUpdateIisSite \
  -SiteName "CustomerApi" \
  -AppPoolName "CustomerApiAppPool" \
  -PhysicalPath "C:\inetpub\CustomerApi" \
  -BindingInformation "*:8080:" \
  -EnvironmentName "Production"
```

### Useful options

- `-SkipTests`: Skips `dotnet test` during deployment.
- `-SelfContained`: Publishes a self-contained deployment.
- `-RuntimeIdentifier win-x64`: Sets the runtime when using self-contained publish.
- `-PublishDirectory`: Changes the local publish output folder.

### Deployment notes

- The script configures the IIS app pool with `No Managed Code` behavior.
- Replace the JWT signing key and any sensitive settings before production deployment.
- Configure HTTPS bindings and certificates separately in IIS for production use.
- If your server uses locked-down permissions, grant the IIS app pool identity read access to the physical deployment folder.

## Database

- Default provider: SQLite
- Default database file: `customer.db`
- Seed behavior: database and initial customer records are created automatically on startup when the database is empty

If you want a different SQLite file location, update `ConnectionStrings:CustomerDatabase` in `appsettings.json` or override it with an environment-specific configuration source.