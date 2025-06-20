namespace RequirementsVillage.Api.Tests.Infrastructure;

public class TestWebApplicationFactory<TProgram>
    : WebApplicationFactory<TProgram> where TProgram : class {
    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.ConfigureServices(services => {
            // Remove the app's ApplicationDbContext registration.
            // Add any test-specific services here
            
            // For now, we'll use the existing in-memory repository
            // Later, we can replace with test doubles as needed
        });

        builder.UseEnvironment("Testing");
    }
}