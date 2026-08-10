using LegacyOrders.DTOs;
using LegacyOrders.Models;
using LegacyOrders.Repositories;
using LegacyOrders.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace LegacyOrders.Tests;

public class OrderServiceTests
{
    [Fact]
    public async Task CreateOrderAsync_ReturnsFailure_WhenCustomerIsInvalid()
    {
        var service = CreateService();

        var response = await service.CreateOrderAsync(new OrderRequest
        {
            CustomerId = 0,
            Items = [new OrderRequestItem { ProductId = 1, Quantity = 1 }]
        }, null, CancellationToken.None);

        Assert.False(response.Success);
        Assert.Equal("Invalid customer", response.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_ReturnsFailure_WhenProductDoesNotExist()
    {
        var repository = new FakeOrderRepository
        {
            Customer = new Customer { Id = 1, Name = "Ada", Email = "ada@example.com", State = "NY", IsVip = false },
            Products = [new Product { Id = 2, Name = "Widget", Price = 10m, Stock = 5 }]
        };
        var service = new OrderService(repository, NullLogger<OrderService>.Instance);

        var response = await service.CreateOrderAsync(new OrderRequest
        {
            CustomerId = 1,
            Items = [new OrderRequestItem { ProductId = 999, Quantity = 1 }]
        }, null, CancellationToken.None);

        Assert.False(response.Success);
        Assert.Equal("Product not found", response.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_ReturnsSuccess_WhenOrderIsValid()
    {
        var repository = new FakeOrderRepository
        {
            Customer = new Customer { Id = 1, Name = "Ada", Email = "ada@example.com", State = "CA", IsVip = true },
            Products = [new Product { Id = 1, Name = "Widget", Price = 10m, Stock = 5 }]
        };
        var service = new OrderService(repository, NullLogger<OrderService>.Instance);

        var response = await service.CreateOrderAsync(new OrderRequest
        {
            CustomerId = 1,
            Country = "US",
            CouponCode = "SAVE10",
            SendEmail = true,
            Items = [new OrderRequestItem { ProductId = 1, Quantity = 2 }]
        }, "tester", CancellationToken.None);

        Assert.True(response.Success);
        Assert.Equal(1, response.OrderId);
        Assert.Equal("Created", response.Status);
        Assert.Single(response.Items);
        Assert.Equal(2, response.Items[0].Quantity);
    }

    private static OrderService CreateService()
    {
        var repository = new FakeOrderRepository();
        return new OrderService(repository, NullLogger<OrderService>.Instance);
    }

    private sealed class FakeOrderRepository : IOrderRepository
    {
        public Customer? Customer { get; init; }

        public List<Product> Products { get; init; } = [];

        public Task<Customer?> GetCustomerAsync(int customerId, CancellationToken cancellationToken)
        {
            return Task.FromResult(Customer?.Id == customerId ? Customer : null);
        }

        public Task<List<Product>> GetProductsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Products);
        }

        public Task<Order> CreateOrderAsync(Order order, Customer customer, IEnumerable<Product> products, string? userName, CancellationToken cancellationToken)
        {
            order.Id = 1;
            return Task.FromResult(order);
        }
    }
}
