using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using QuotesApi.Models;
using QuotesApi.Repositories;
using Microsoft.AspNetCore.TestHost;


namespace QuotesApi.Tests;

public class CollectionCancellationTests
{
    [Fact]
    public async Task Request_can_be_cancelled_mid_request()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                    {
                        services.RemoveAll<ICollectionRepository>();
                        services.AddScoped<ICollectionRepository, BlockingCollectionRepository>();
                    });
            });

        using var client = factory.CreateClient();

        using var cts = new CancellationTokenSource();

        var requestTask = client.PostAsync(
            "/api/collections/1/items/1",
            null,
            cts.Token);

        // Make sure the request has reached the repository.
        await BlockingCollectionRepository.Started.Task;

        // Cancel while the request is still running.
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await requestTask);
    }

    private sealed class BlockingCollectionRepository : ICollectionRepository
    {
        public static readonly TaskCompletionSource Started =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<Collection?> GetById(
            int id,
            CancellationToken cancellationToken)
        {
            Started.TrySetResult();

            // Keep the request running until cancellation occurs.
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

            return null;
        }

        public Task Add(
            Collection collection,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task Update(
            Collection collection,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task Delete(
            Collection collection,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}