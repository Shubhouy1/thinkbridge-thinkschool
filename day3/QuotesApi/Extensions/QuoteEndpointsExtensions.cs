using QuotesApi.Models;
using QuotesApi.Repositories;

namespace QuotesApi.Extensions;

public static class QuoteEndpointsExtensions
{
    public static IEndpointRouteBuilder MapQuoteEndpoints(
        this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/quotes");

        group.MapGet("/", async (
            int page,
            int size,
            IQuoteRepository repo,
            ILogger<Program> logger,
            CancellationToken ct) =>
        {
            page = page < 1 ? 1 : page;
            size = size < 1 ? 10 : size;

            logger.LogInformation(
                "Getting quotes. Page: {Page}, Size: {Size}",
                page, size);

            return Results.Ok(
                await repo.GetAllAsync(page, size, ct));
        });

        group.MapGet("/{id:int}", async (
            int id,
            IQuoteRepository repo,
            ILogger<Program> logger,
            CancellationToken ct) =>
        {
            logger.LogInformation("Getting quote {QuoteId}", id);

            var quote = await repo.GetByIdAsync(id, ct);

            return quote is null
                ? Results.NotFound()
                : Results.Ok(quote);
        });

        group.MapPost("/", async (
            QuoteCreateRequest request,
            IQuoteRepository repo,
            ILogger<Program> logger,
            CancellationToken ct) =>
        {
            var errors = new Dictionary<string, string[]>();
            var (quote, error) = Quote.Create(request.Author, request.Text);

            if (error is not null)
            {
                errors[error.PropertyName] = [error.Message];
                return Results.ValidationProblem(errors);
            }

            logger.LogInformation(
                "Creating quote by {Author}",
                quote!.Author);

            var created = await repo.AddAsync(quote, ct);

            return Results.Created(
                $"/api/quotes/{created.Id}",
                created);
        });

        group.MapDelete("/{id:int}", async (
            int id,
            IQuoteRepository repo,
            ILogger<Program> logger,
            CancellationToken ct) =>
        {
            logger.LogInformation("Deleting quote {QuoteId}", id);

            var deleted = await repo.DeleteAsync(id, ct);

            return deleted
                ? Results.NoContent()
                : Results.NotFound();
        });

        return app;
    }
}