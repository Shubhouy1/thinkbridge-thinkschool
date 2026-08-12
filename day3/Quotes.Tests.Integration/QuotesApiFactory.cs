using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using QuotesApi.Data;
using QuotesApi.Infrastructure;
using QuotesApi.Models;

namespace Quotes.Tests.Integration;

public class QuotesApiFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    static QuotesApiFactory()
    {
        Environment.SetEnvironmentVariable(
            "Jwt__Key",
            "test-secret-key-for-integration-tests-123456789");

        Environment.SetEnvironmentVariable(
            "Jwt__Issuer",
            "QuotesApi");

        Environment.SetEnvironmentVariable(
            "Jwt__Audience",
            "QuotesApiClient");

        Environment.SetEnvironmentVariable(
            "Jwt__ExpiresInMinutes",
            "15");

        Environment.SetEnvironmentVariable(
            "Entra__TenantId",
            "test-tenant-id");

        Environment.SetEnvironmentVariable(
            "Entra__Audience",
            "test-audience");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var dbContextDescriptor =
                services.SingleOrDefault(
                    d => d.ServiceType ==
                         typeof(DbContextOptions<QuotesDbContext>));

            if (dbContextDescriptor is not null)
                services.Remove(dbContextDescriptor);

            var clockDescriptor =
                services.SingleOrDefault(
                    d => d.ServiceType == typeof(IClock));

            if (clockDescriptor is not null)
                services.Remove(clockDescriptor);

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddSingleton(_connection);

            services.AddDbContext<QuotesDbContext>(options =>
                options.UseSqlite(_connection));

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

            var serviceProvider = services.BuildServiceProvider();

            using var scope = serviceProvider.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<QuotesDbContext>();

            db.Database.Migrate();

            if (!db.Users.Any(u =>
                    u.Email == "test@example.com"))
            {
                db.Users.Add(new User
                {
                    Email = "test@example.com",
                    PasswordHash =
                        BCrypt.Net.BCrypt.HashPassword(
                            "Password123!")
                });
            }

            if (!db.Users.Any(u =>
                    u.Email == "test2@example.com"))
            {
                db.Users.Add(new User
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }

        base.Dispose(disposing);
    }
}