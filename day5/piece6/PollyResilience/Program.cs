
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;

var services = new ServiceCollection();

services.AddHttpClient("resilient-service")
    .AddResilienceHandler("default", builder =>
    {
        builder.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            OnRetry = args =>
            {
                Console.WriteLine(
                    $"Retry #{args.AttemptNumber + 1}: " +
                    $"{args.Outcome.Exception?.Message ?? args.Outcome.Result?.StatusCode.ToString()}");

                return default;
            }
        });

        builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(30),
            MinimumThroughput = 10,
            BreakDuration = TimeSpan.FromSeconds(30)
        });

        builder.AddTimeout(TimeSpan.FromSeconds(10));
    });

var provider = services.BuildServiceProvider();

var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
var client = clientFactory.CreateClient("resilient-service");

try
{
    Console.WriteLine("Calling failing endpoint...");

    await client.GetAsync("https://localhost:59999/fail");
}
catch (Exception ex)
{
    Console.WriteLine($"Final failure: {ex.Message}");
}