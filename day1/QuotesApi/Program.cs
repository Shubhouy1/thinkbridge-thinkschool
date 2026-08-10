using Microsoft.EntityFrameworkCore;
using QuotesApi.Data;
using QuotesApi.Models;
using QuotesApi.Repositories;
using QuotesApi.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<QuotesDbContext>(options =>
    options.UseSqlite("Data Source=quotes.db"));

builder.Services.AddScoped<IQuoteRepository, QuoteRepository>();
builder.Services.AddScoped<ICollectionRepository, CollectionRepository>();

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

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

app.MapPost("/api/quotes", async (
    Quote quote,
    IQuoteRepository repo,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(quote.Author) ||
        string.IsNullOrWhiteSpace(quote.Text))
    {
        return Results.BadRequest(new
        {
            error = "Author and text are required."
        });
    }

    var created = await repo.AddAsync(
        quote,
        cancellationToken);

    return Results.Created(
        $"/api/quotes/{created.Id}",
        created);
});

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
});

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

// Add quote to collection
app.MapPost("/api/collections/{id}/items/{quoteId}", async (
    int id,
    int quoteId,
    ICollectionRepository repo,
    CancellationToken cancellationToken) =>
{
    var collection = await repo.GetById(id, cancellationToken);

    if (collection is null)
        return Results.NotFound();

    collection.AddItem(quoteId);

    await repo.Update(collection, cancellationToken);

    return Results.Ok(collection);
});

// Remove quote from collection
app.MapDelete("/api/collections/{id}/items/{quoteId}", async (
    int id,
    int quoteId,
    ICollectionRepository repo,
    CancellationToken cancellationToken) =>
{
    var collection = await repo.GetById(id, cancellationToken);

    if (collection is null)
        return Results.NotFound();

    collection.RemoveItem(quoteId);

    await repo.Update(collection, cancellationToken);

    return Results.NoContent();
});

app.Run();