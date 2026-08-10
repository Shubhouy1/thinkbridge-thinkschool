using System.Net;
using System.Net.Http.Json;
using LegacyOrders.DTOs;
using LegacyOrders.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace LegacyOrders.Tests;

public class OrdersApiTests : IClassFixture<OrdersApiFactory>
{
    private readonly OrdersApiFactory _factory;

    public OrdersApiTests(OrdersApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostToOrders_CreatesOrderAndPersistsIt()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/orders", new
        {
            customerId = 1,
            country = "US",
            couponCode = string.Empty,
            sendEmail = false,
            items = new[]
            {
                new { productId = 1, quantity = 2 }
            }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var payload = await response.Content.ReadFromJsonAsync<OrderCreateResponse>();
        Assert.NotNull(payload);
        Assert.True(payload!.Success);
        Assert.Equal(1, payload.OrderId);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var order = await db.Orders.Include(x => x.Items).SingleAsync(x => x.Id == payload.OrderId);

        Assert.Single(order.Items);
        Assert.Equal(2, order.Items[0].Quantity);
    }
}

public sealed class OrdersApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");

    public OrdersApiFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll(typeof(DbContextOptions<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));
            services.AddSingleton(_connection);
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            db.Customers.Add(new Customer
            {
                Id = 1,
                Name = "Ada",
                Email = "ada@example.com",
                State = "NY",
                IsVip = false
            });
            db.Products.Add(new Product
            {
                Id = 1,
                Name = "Widget",
                Price = 10m,
                Stock = 10
            });
            db.SaveChanges();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
