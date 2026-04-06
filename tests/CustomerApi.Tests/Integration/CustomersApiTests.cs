using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CustomerApi.Contracts.Requests;
using CustomerApi.Contracts.Responses;
using Microsoft.AspNetCore.Mvc.Testing;

namespace CustomerApi.Tests.Integration;

public sealed class CustomersApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public CustomersApiTests(WebApplicationFactory<Program> factory)
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
