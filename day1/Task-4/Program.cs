using LegacyOrders.Models;
using LegacyOrders.Repositories;
using LegacyOrders.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=orders.db"));
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IShippingStrategy, DefaultShippingStrategy>();
builder.Services.AddScoped<ITaxStrategy, DefaultTaxStrategy>();
builder.Services.AddScoped<IDiscountStrategy, DefaultDiscountStrategy>();
builder.Services.AddScoped<IOrderService, OrderService>();

var app = builder.Build();

app.MapControllers();
app.Run();

public partial class Program
{
}
