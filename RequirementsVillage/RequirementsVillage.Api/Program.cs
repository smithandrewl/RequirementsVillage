var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Serve static files from wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// API endpoints
app.MapGet("/api/health", () => new {
    Status    = "Healthy", 
    Timestamp = DateTime.UtcNow 
});

// SPA fallback - must be last
app.MapFallbackToFile("index.html");

app.Run();
