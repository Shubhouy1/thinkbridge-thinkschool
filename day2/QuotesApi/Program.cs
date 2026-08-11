using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuotesApi.Data;
using QuotesApi.Infrastructure;
using QuotesApi.Models;
using QuotesApi.Repositories;
using QuotesApi.Middleware;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<QuotesDbContext>(options =>
    options.UseSqlite("Data Source=quotes.db"));

builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();
builder.Services.AddSingleton<IClock, SystemClock>();

// JWT configuration
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT key is not configured.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is not configured.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is not configured.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)),

            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,

            ValidateAudience = true,
            ValidAudience = jwtAudience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/quotes", async (
    int page,
    int size,
    IQuoteRepository repo,
    CancellationToken cancellationToken) =>
{
    page = page < 1 ? 1 : page;
    size = size < 1 ? 10 : size;

    var quotes = await repo.GetAllAsync(
        page,
        size,
        cancellationToken);

    return Results.Ok(quotes);
});

app.MapGet("/api/quotes/{id}", async (
    int id,
    IQuoteRepository repo,
    CancellationToken cancellationToken) =>
{
    var quote = await repo.GetByIdAsync(
        id,
        cancellationToken);

    return quote is null
        ? Results.NotFound()
        : Results.Ok(quote);
});

// DELETE quote - protected
app.MapDelete("/api/quotes/{id}", async (
    int id,
    IQuoteRepository repo,
    CancellationToken cancellationToken) =>
{
    var deleted = await repo.DeleteAsync(
        id,
        cancellationToken);

    return deleted
        ? Results.NoContent()
        : Results.NotFound();
})
.RequireAuthorization();

// Create collection
app.MapPost("/api/collections", async (
    Collection collection,
    ICollectionRepository repo,
    CancellationToken cancellationToken) =>
{
    await repo.Add(collection, cancellationToken);

    return Results.Created(
        $"/api/collections/{collection.Id}",
        collection);
});

app.MapPost("/api/auth/login", async (
    LoginRequest request,
    QuotesDbContext db,
    IConfiguration configuration,
    CancellationToken cancellationToken) =>
{
    var user = await db.Users
        .FirstOrDefaultAsync(
            u => u.Email == request.Email,
            cancellationToken);

    if (user is null ||
        !BCrypt.Net.BCrypt.Verify(
            request.Password,
            user.PasswordHash))
    {
        return Results.Unauthorized();
    }

    var jwtKey = configuration["Jwt:Key"]
        ?? throw new InvalidOperationException(
            "JWT key is not configured.");

    var jwtIssuer = configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException(
            "JWT issuer is not configured.");

    var jwtAudience = configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException(
            "JWT audience is not configured.");

    var expiresInMinutes =
        configuration.GetValue<int>("Jwt:ExpiresInMinutes");

    var expiresAt = DateTime.UtcNow.AddMinutes(
        expiresInMinutes);

    var claims = new[]
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            user.Id.ToString()),

        new Claim(
            ClaimTypes.Email,
            user.Email)
    };

    var credentials = new SigningCredentials(
        new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)),
        SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: expiresAt,
        signingCredentials: credentials);

    var accessToken = new JwtSecurityTokenHandler()
        .WriteToken(token);

    var refreshToken = Convert.ToBase64String(
        RandomNumberGenerator.GetBytes(32));

    return Results.Ok(new
    {
        access_token = accessToken,
        refresh_token = refreshToken,
        expires_in = (int)TimeSpan
            .FromMinutes(expiresInMinutes)
            .TotalSeconds
    });
});

// Create quote - protected
app.MapPost("/api/quotes", async (
    QuoteCreateRequest request,
    IQuoteRepository repo,
    CancellationToken cancellationToken) =>
{
    var (quote, error) = Quote.Create(
        request.Author,
        request.Text);

    if (error is not null)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                [error.PropertyName] = [error.Message]
            });
    }

    var created = await repo.AddAsync(
        quote!,
        cancellationToken);

    return Results.Created(
        $"/api/quotes/{created.Id}",
        created);
})
.RequireAuthorization();

// Remove quote from collection
app.MapDelete("/api/collections/{id}/items/{quoteId}", async (
    int id,
    int quoteId,
    ICollectionRepository repo,
    CancellationToken cancellationToken) =>
{
    var collection = await repo.GetById(
        id,
        cancellationToken);

    if (collection is null)
        return Results.NotFound();

    collection.RemoveItem(quoteId);

    await repo.Update(
        collection,
        cancellationToken);

    return Results.NoContent();
});

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<QuotesDbContext>();

    if (!await db.Users.AnyAsync())
    {
        db.Users.Add(new User
        {
            Email = "test@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!")
        });

        await db.SaveChangesAsync();
    }
}

app.Run();