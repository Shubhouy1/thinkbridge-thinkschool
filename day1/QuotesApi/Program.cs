using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<QuotesDbContext>(options =>
    options.UseSqlite("Data Source=quotes.db"));

var app = builder.Build();

app.Run();