using Portic.Core.DependencyInjection;
using Portic.Gateway.Endpoints;
using Portic.Providers.Stub;

var builder = WebApplication.CreateBuilder(args);

// Provider-neutral core + the local stub adapter. To use a real provider, swap AddStubProvider()
// for that provider's adapter registration — no change to the core or the endpoints.
builder.Services.AddPorticCore(builder.Configuration);
builder.Services.AddStubProvider();

var app = builder.Build();

app.MapMessagesEndpoints();
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithName("Health");

app.Run();

// Exposed so the integration test host (WebApplicationFactory) can boot the real app.
public partial class Program;
