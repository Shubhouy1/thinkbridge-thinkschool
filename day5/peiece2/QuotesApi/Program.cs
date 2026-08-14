var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    status = "Healthy"
}));
app.MapGet("/api/quotes", () => Results.Ok(new
{
    message = "Quotes API is running"
}));
app.Run();