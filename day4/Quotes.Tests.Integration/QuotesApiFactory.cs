using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuotesApi.Data;
using QuotesApi.Infrastructure;
using QuotesApi.Models;

namespace Quotes.Tests.Integration;

public class QuotesApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public QuotesApiFactory(string serverConnectionString)
    {
        var builder = new SqlConnectionStringBuilder(
            serverConnectionString)
        {
            InitialCatalog = $"QuotesTest_{Guid.NewGuid():N}"
        };

        _connectionString = builder.ConnectionString;
    }

    static QuotesApiFactory()
    {
        Environment.SetEnvironmentVariable(
            "Jwt__Key",
            "test-secret-key-for-integration-tests-12345678901234567890");

        Environment.SetEnvironmentVariable(
            "Jwt__Issuer",
            "QuotesApi");

        Environment.SetEnvironmentVariable(
            "Jwt__Audience",
            "QuotesApiClient");

        Environment.SetEnvironmentVariable(
            "Jwt__ExpiresInMinutes",
            "60");

        Environment.SetEnvironmentVariable(
            "Entra__TenantId",
            "test-tenant-id");

        Environment.SetEnvironmentVariable(
            "Entra__Audience",
            "test-audience");
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the application's DbContext registration.
            services.RemoveAll<DbContextOptions<QuotesDbContext>>();
            services.RemoveAll<QuotesDbContext>();

            // Replace the application's clock.
            services.RemoveAll<IClock>();

            services.AddSingleton<IClock>(
                new FakeClock
                {
                    UtcNow = new DateTimeOffset(
                        2026,
                        8,
                        12,
                        10,
                        0,
                        0,
                        TimeSpan.Zero)
                });

            // Use the SQL Server Testcontainer database.
            services.AddDbContext<QuotesDbContext>(options =>
            {
                options.UseSqlServer(_connectionString);
            });

            // Build a temporary provider so we can initialize
            // the fresh SQL Server database.
            using var serviceProvider =
                services.BuildServiceProvider();

            using var scope =
                serviceProvider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<QuotesDbContext>();

            // IMPORTANT:
            // Do NOT use SQLite migrations here.
            // Create the SQL Server database directly from
            // the current EF Core model.
            db.Database.EnsureCreated();

            // Seed test user 1.
            if (!db.Users.Any(
                    u => u.Email == "test@example.com"))
            {
                db.Users.Add(
                    new User
                    {
                        Email = "test@example.com",
                        PasswordHash =
                            BCrypt.Net.BCrypt.HashPassword(
                                "Password123!")
                    });
            }

            // Seed test user 2.
            if (!db.Users.Any(
                    u => u.Email == "test2@example.com"))
            {
                db.Users.Add(
                    new User
                    {
                        Email = "test2@example.com",
                        PasswordHash =
                            BCrypt.Net.BCrypt.HashPassword(
                                "Password123!")
                    });
            }

            db.SaveChanges();
        });
    }
}