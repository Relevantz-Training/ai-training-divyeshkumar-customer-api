# Customer API

Customer API is a .NET 8 ASP.NET Core Web API that exposes secured CRUD operations for a basic customer details table. The current implementation uses a mock-backed in-memory repository so the API contract, security model, Swagger documentation, and automated tests can be exercised before a real database is introduced.

## Solution structure

- `src/CustomerApi`: Main API project.
- `tests/CustomerApi.Tests`: Unit and integration tests.
- `src/CustomerApi/Controllers`: API endpoints.
- `src/CustomerApi/Services`: Business logic and token generation.
- `src/CustomerApi/Repositories`: Mock persistence layer.
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
- Mock customer and mock user data for local testing.

## Mock users for local testing

Use the token endpoint to authenticate with these seeded users:

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@customerapi.local` | `Admin123!` |
| Support | `support@customerapi.local` | `Support123!` |

## Main endpoints

- `POST /api/auth/token`: Returns a JWT for a mock user.
- `GET /api/customers`: Returns all customers. Requires `Admin` or `Support` role.
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
- Replace the development signing key before any shared or production deployment.
- Keep production secrets out of source control by using user secrets, environment variables, or a secrets manager.
- Swagger is enabled in development by default. Keep production exposure limited unless there is a business need.
- The API returns standardized problem details and avoids exposing internal stack traces.

## Running locally

1. Install the .NET 8 SDK.
2. Restore dependencies with `dotnet restore`.
3. Build the solution with `dotnet build CustomerApi.sln`.
4. Run the API with `dotnet run --project src/CustomerApi/CustomerApi.csproj`.
5. Open Swagger at the local application URL, authenticate through `POST /api/auth/token`, and use the returned bearer token in Swagger.

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

## Future database integration

The current repository abstraction is intentionally database-agnostic. A later phase can replace the in-memory repository with EF Core and a real customer details table without changing the controller contract.