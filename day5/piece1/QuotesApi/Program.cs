using System.Diagnostics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);
var activitySource = new ActivitySource("Day5.Piece1");

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddSource("Day5.Piece1")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }));

var app = builder.Build();
app.MapGet("/api/quotes", () =>
{
    using var activity = activitySource.StartActivity("slow-operation");
    return Results.Ok(new
    {
        message = "Quotes loaded"
    });
});

app.Run();