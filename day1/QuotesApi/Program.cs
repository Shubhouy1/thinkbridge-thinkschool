using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Models;
using QuotesApi.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<QuotesDbContext>(options =>
    options.UseSqlite("Data Source=quotes.db"));

builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();

var app = builder.Build();

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
    var quote = await repo.GetByIdAsync(id, cancellationToken);

    return quote is null
        ? Results.NotFound()
        : Results.Ok(quote);
});

app.MapPost("/api/quotes", async (
    Quote quote,
    IQuoteRepository repo,
    CancellationToken cancellationToken) =>
{
    var created = await repo.AddAsync(quote, cancellationToken);

    return Results.Created(
        $"/api/quotes/{created.Id}",
        created);
});

app.MapDelete("/api/quotes/{id}", async (
    int id,
    IQuoteRepository repo,
    CancellationToken cancellationToken) =>
{
    var deleted = await repo.DeleteAsync(id, cancellationToken);

    return deleted
        ? Results.NoContent()
        : Results.NotFound();
});

app.Run();