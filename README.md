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
- `src/CustomerApi/Security`: JWT options and API key authentication handler.

## Implemented features

- CRUD endpoints for customer details.
- .NET 8 Web API with a layered structure.
- JWT bearer authentication (username/password via `POST /api/auth/token`).
- API key authentication via `X-Api-Key` header (machine-to-machine).
- Role-based authorization — both auth schemes supported on all customer endpoints.
- Swagger with bearer-token and API key support.
- Global exception handling using problem details responses.
- Rate limiting (100 req/min per API key prefix or per IP).
- Security headers middleware.
- Separate automated test project.
- SQLite-backed customer persistence with automatic seed data (10 customers).
- Mock users for local authentication testing.

## Mock users for local testing

Use the token endpoint to authenticate with these seeded users:

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@customerapi.local` | `Admin123!` |
| Support | `support@customerapi.local` | `Support123!` |

## Main endpoints

### Auth
- `POST /api/auth/token`: Returns a JWT for a mock user.

### Customers (JWT Bearer or X-Api-Key)
- `GET /api/customers`: Returns all customers. Requires `Admin` or `Support` role.
- `GET /api/customers/{id}`: Returns one customer. Requires `Admin` or `Support` role.
- `POST /api/customers`: Creates a customer. Requires `Admin` role.
- `PUT /api/customers/{id}`: Updates a customer. Requires `Admin` role.
- `DELETE /api/customers/{id}`: Deletes a customer. Requires `Admin` role.

### API Keys (JWT Bearer, Admin only)
- `POST /api/apikeys`: Creates a new API key — the raw key is shown once, save it immediately.
- `GET /api/apikeys`: Lists all API keys (prefix + metadata, no raw keys).
- `DELETE /api/apikeys/{id}`: Revokes (deactivates) an API key.

### Health
- `GET /health`: Health check endpoint (anonymous).

## Running locally

1. Install the .NET 8 SDK.
2. Restore dependencies with `dotnet restore`.
3. Build the solution with `dotnet build CustomerApi.sln`.
4. Delete any existing `customer.db` file if you want fresh seed data.
5. Run the API with `dotnet run --project src/CustomerApi/CustomerApi.csproj`.
6. The API listens on **http://localhost:5100** in Development mode.
7. On first run, the API creates the local SQLite file and seeds 10 default customer records automatically.
8. Open Swagger at **http://localhost:5100/swagger**.

## Example requests

### Get a JWT token

```http
POST http://localhost:5100/api/auth/token
Content-Type: application/json

{
  "email": "admin@customerapi.local",
  "password": "Admin123!"
}
```

### Create an API key (Admin JWT required)

```http
POST http://localhost:5100/api/apikeys
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "name": "Mobile App",
  "roles": ["Admin"],
  "expiresAtUtc": null
}
```

Response includes `rawKey` — **save it now**, it is never retrievable again.

### Call a customer endpoint using an API key

```http
GET http://localhost:5100/api/customers
X-Api-Key: {raw_key_from_creation}
```

### Create a customer (JWT or API key)

```http
POST http://localhost:5100/api/customers
X-Api-Key: {raw_key}
Content-Type: application/json

{
  "firstName": "Taylor",
  "lastName": "Morgan",
  "email": "taylor.morgan@example.com",
  "phoneNumber": "+1-555-222-3333",
  "isActive": true
}
```

### Revoke an API key

```http
DELETE http://localhost:5100/api/apikeys/{id}
Authorization: Bearer {jwt_token}
```

## API key integration guide

This section explains how an external application or service integrates with the Customer API using API keys instead of username/password login.

### Overview

```
Your App  ──── X-Api-Key: <raw_key> ────►  Customer API
                                               │
                                         validates prefix
                                         + SHA-256 hash
                                               │
                                          returns data
```

API key authentication uses a single HTTP header — no OAuth flows, no token refresh, no session management.

---

### Step 1 — Provision an API key (one-time, Admin only)

An administrator must log in with their JWT credentials and create a key for your application.

**1a. Get an Admin JWT**

```http
POST http://localhost:5100/api/auth/token
Content-Type: application/json

{
  "email": "admin@customerapi.local",
  "password": "Admin123!"
}
```

Response:
```json
{
  "accessToken": "eyJhbGci..."
}
```

**1b. Create the API key**

Choose roles based on what access the application needs:
- `"Admin"` — full read/write/delete access
- `"Support"` — read-only access

```http
POST http://localhost:5100/api/apikeys
Authorization: Bearer eyJhbGci...
Content-Type: application/json

{
  "name": "My Integration App",
  "roles": ["Admin"],
  "expiresAtUtc": null
}
```

Response:
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "My Integration App",
  "keyPrefix": "a1b2c3d4",
  "roles": ["Admin"],
  "isActive": true,
  "createdAtUtc": "2026-05-09T12:00:00Z",
  "expiresAtUtc": null,
  "rawKey": "a1b2c3d4e5f6..."
}
```

> **Important:** Copy `rawKey` immediately. It is shown **once only** and never stored. If lost, delete the key and create a new one.

---

### Step 2 — Store the key securely in your application

Never hardcode the raw key in source code. Use one of:

| Environment | Recommended storage |
|-------------|---------------------|
| Local dev | `.env` file (git-ignored) or user secrets |
| CI/CD | Pipeline secret / environment variable |
| Production server | Environment variable or secrets manager (Azure Key Vault, AWS Secrets Manager, etc.) |

Read it at runtime:
```csharp
var apiKey = Environment.GetEnvironmentVariable("CUSTOMER_API_KEY");
```

---

### Step 3 — Call the API

Pass the raw key in the `X-Api-Key` request header on every call. No token refresh is needed.

#### cURL
```bash
curl -X GET "http://localhost:5100/api/customers" \
  -H "X-Api-Key: a1b2c3d4e5f6..."
```

#### HTTP (any language)
```http
GET http://localhost:5100/api/customers
X-Api-Key: a1b2c3d4e5f6...
accept: application/json
```

#### C# (HttpClient)
```csharp
var client = new HttpClient();
client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);

var response = await client.GetAsync("http://localhost:5100/api/customers");
var customers = await response.Content.ReadFromJsonAsync<List<CustomerResponse>>();
```

#### JavaScript (fetch)
```javascript
const response = await fetch("http://localhost:5100/api/customers", {
  headers: {
    "X-Api-Key": process.env.CUSTOMER_API_KEY,
    "Accept": "application/json"
  }
});
const customers = await response.json();
```

#### Python (requests)
```python
import requests, os

headers = {"X-Api-Key": os.environ["CUSTOMER_API_KEY"]}
r = requests.get("http://localhost:5100/api/customers", headers=headers)
customers = r.json()
```

---

### Step 4 — Create / update / delete customers

```http
POST http://localhost:5100/api/customers
X-Api-Key: a1b2c3d4e5f6...
Content-Type: application/json

{
  "firstName": "Taylor",
  "lastName": "Morgan",
  "email": "taylor.morgan@example.com",
  "phoneNumber": "+1-555-222-3333",
  "isActive": true
}
```

```http
PUT http://localhost:5100/api/customers/{id}
X-Api-Key: a1b2c3d4e5f6...
Content-Type: application/json

{
  "firstName": "Taylor",
  "lastName": "Morgan",
  "email": "taylor.morgan@example.com",
  "phoneNumber": "+1-555-333-4444",
  "isActive": true
}
```

```http
DELETE http://localhost:5100/api/customers/{id}
X-Api-Key: a1b2c3d4e5f6...
```

---

### Step 5 — Handle API responses

| Status | Meaning | Action |
|--------|---------|--------|
| `200 OK` | Success | Process the response body |
| `201 Created` | Resource created | Use the `id` from the response |
| `204 No Content` | Deleted successfully | No body |
| `400 Bad Request` | Validation failed | Fix the request body |
| `401 Unauthorized` | Key missing, invalid, revoked, or expired | Check the key and header name |
| `403 Forbidden` | Key exists but role insufficient | Use a key with the required role |
| `404 Not Found` | Customer ID does not exist | Verify the ID |
| `409 Conflict` | Email already in use | Use a different email |
| `429 Too Many Requests` | Rate limit exceeded (100 req/min per key) | Back off and retry |
| `500 Internal Server Error` | Unexpected server error | Retry or contact the API owner |

---

### Step 6 — Rotate or revoke a key

**List all keys** (to find the key ID):
```http
GET http://localhost:5100/api/apikeys
Authorization: Bearer eyJhbGci...
```

**Revoke a key** (soft-delete, takes effect immediately):
```http
DELETE http://localhost:5100/api/apikeys/{id}
Authorization: Bearer eyJhbGci...
```

After revoking, create a new key and update the secret in your application's environment. The old key returns `401` immediately upon revocation.

---

### Key limitations

- API keys **cannot** call `POST/GET/DELETE /api/apikeys` — key management always requires an Admin JWT to prevent privilege escalation.
- Rate limit: **100 requests per minute** per API key prefix. Exceeding this returns `429`.
- If `expiresAtUtc` was set during creation, the key stops working after that date and returns `401`.



## Security notes

- JWT configuration is stored under the `Jwt` section in `appsettings.json`.
- SQLite connection string is stored under `ConnectionStrings:CustomerDatabase`.
- Replace the development signing key before any shared or production deployment.
- Keep production secrets out of source control by using user secrets, environment variables, or a secrets manager.
- Swagger is enabled in development by default. Keep production exposure limited unless there is a business need (`Swagger:EnableInProduction: true`).
- HTTPS redirect is disabled in Development to simplify local testing. Enable it in production.
- The API returns standardized problem details and avoids exposing internal stack traces.

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
- Seed behavior: database and initial records are created automatically on startup when the database is empty.
- Seed data: 10 customers across active/inactive states; no API keys are seeded (create them via `POST /api/apikeys`).