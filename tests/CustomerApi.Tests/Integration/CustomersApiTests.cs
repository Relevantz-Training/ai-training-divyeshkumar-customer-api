using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CustomerApi.Tests.Integration;

public sealed class CustomersApiTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public CustomersApiTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CustomersEndpoint_RejectsAnonymousRequests()
    {
        var response = await _client.GetAsync("/api/customers");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task TokenEndpoint_ReturnsJwtForMockAdmin()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/token", new LoginRequest
        {
            Email = "admin@customerapi.local",
            Password = "Admin123!"
        });

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();

        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload.AccessToken));
    }

    [Fact]
    public async Task CustomersEndpoint_ReturnsSeededCustomersForAuthorizedUser()
    {
        var tokenResponse = await _client.PostAsJsonAsync("/api/auth/token", new LoginRequest
        {
            Email = "support@customerapi.local",
            Password = "Support123!"
        });

        var token = await tokenResponse.Content.ReadFromJsonAsync<AccessTokenResponse>();
        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token!.AccessToken);

        var response = await _client.GetAsync("/api/customers");

        response.EnsureSuccessStatusCode();
        var customers = await response.Content.ReadFromJsonAsync<List<CustomerResponse>>(new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(customers);
        Assert.True(customers.Count >= 3);
    }
}

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _databasePath = Path.Combine(Path.GetTempPath(), $"customer-api-tests-{Guid.NewGuid():N}.db");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:CustomerDatabase"] = $"Data Source={_databasePath}"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}
