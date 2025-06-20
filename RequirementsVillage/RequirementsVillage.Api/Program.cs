using RequirementsVillage.Api.Endpoints;
using RequirementsVillage.Api.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDomainServices();

var app = builder.Build();

// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// API endpoints
app.MapGet("/api/health", () => new {
    Status    = "Healthy", 
    Timestamp = DateTime.UtcNow 
});

app.MapProjectEndpoints();

// SPA fallback - must be last
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program { }
