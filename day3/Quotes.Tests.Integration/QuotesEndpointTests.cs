using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using System.Text.Json.Serialization;

namespace Quotes.Tests.Integration;

public class QuotesEndpointTests
{
    private static QuotesApiFactory CreateFactory()
    {
        return new QuotesApiFactory();
    }

    private static async Task<string> LoginAsync(
        HttpClient client,
        string email)
    {
        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email,
                password = "Password123!"
            });

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<TokenResponse>();

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();

        return result.AccessToken;
    }

private sealed class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
}
    private sealed class QuoteResponse
    {
        public int Id { get; set; }
    }

    [Fact]
    public async Task GetQuotes_WithoutAuthentication_ReturnsOk()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/quotes?page=1&size=10");

        response.StatusCode
            .Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetQuote_InvalidId_ReturnsNotFound()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync(
            "/api/quotes/999999");

        response.StatusCode
            .Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateQuote_AuthenticatedUser_ReturnsCreated()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var token = await LoginAsync(
            client,
            "test@example.com");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                author = "Integration Test",
                text = "A valid integration quote"
            });

        response.StatusCode
            .Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateQuote_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                author = "Anonymous",
                text = "Should not be created"
            });

        response.StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateQuote_InvalidData_ReturnsBadRequest()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var token = await LoginAsync(
            client,
            "test@example.com");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                author = "",
                text = ""
            });

        response.StatusCode
            .Should().Be(HttpStatusCode.BadRequest);

        response.Content.Headers.ContentType?.MediaType
            .Should().Be("application/problem+json");
    }

    [Fact]
    public async Task DeleteQuote_WithoutAuthentication_ReturnsUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync(
            "/api/quotes/999999");

        response.StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
public async Task DeleteQuote_NonExistingQuote_ReturnsForbidden()
{
    using var factory = CreateFactory();
    using var client = factory.CreateClient();

    var token = await LoginAsync(
        client,
        "test@example.com");

    client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);

    var response = await client.DeleteAsync(
        "/api/quotes/999999");

    response.StatusCode
        .Should().Be(HttpStatusCode.Forbidden);
}

    [Fact]
    public async Task DeleteQuote_OwnQuote_ReturnsNoContent()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var token = await LoginAsync(
            client,
            "test@example.com");

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await client.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                author = "Owner",
                text = "Quote owned by user one"
            });

        createResponse.StatusCode
            .Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content
            .ReadFromJsonAsync<QuoteResponse>();

        created.Should().NotBeNull();

        var deleteResponse = await client.DeleteAsync(
            $"/api/quotes/{created!.Id}");

        deleteResponse.StatusCode
            .Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteQuote_WrongOwner_ReturnsForbidden()
    {
        using var factory = CreateFactory();

        // User 1 creates the quote
        using var client1 = factory.CreateClient();

        var token1 = await LoginAsync(
            client1,
            "test@example.com");

        client1.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token1);

        var createResponse = await client1.PostAsJsonAsync(
            "/api/quotes",
            new
            {
                author = "User One",
                text = "Owned by user one"
            });

        createResponse.StatusCode
            .Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content
            .ReadFromJsonAsync<QuoteResponse>();

        created.Should().NotBeNull();

        // User 2 uses a completely separate HttpClient
        using var client2 = factory.CreateClient();

        var token2 = await LoginAsync(
            client2,
            "test2@example.com");

        client2.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token2);

        var deleteResponse = await client2.DeleteAsync(
            $"/api/quotes/{created!.Id}");

        deleteResponse.StatusCode
            .Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOk()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "test@example.com",
                password = "Password123!"
            });

        response.StatusCode
            .Should().Be(HttpStatusCode.OK);

        var result = await response.Content
            .ReadFromJsonAsync<TokenResponse>();

        result.Should().NotBeNull();
        result!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/login",
            new
            {
                email = "test@example.com",
                password = "WrongPassword!"
            });

        response.StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_InvalidToken_ReturnsUnauthorized()
    {
        using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/auth/refresh",
            new
            {
                refreshToken = "invalid-refresh-token"
            });

        response.StatusCode
            .Should().Be(HttpStatusCode.Unauthorized);
    }
}