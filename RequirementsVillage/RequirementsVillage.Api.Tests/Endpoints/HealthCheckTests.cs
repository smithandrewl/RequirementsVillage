namespace RequirementsVillage.Api.Tests.Endpoints;

public class HealthCheckTests : IClassFixture<TestWebApplicationFactory<Program>> {
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public HealthCheckTests(TestWebApplicationFactory<Program> factory) {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOkResult() {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthCheck_ReturnsExpectedJson() {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var health = await response.Content.ReadFromJsonAsync<HealthCheckResponse>();
        health.Should().NotBeNull();
        health!.Status.Should().Be("Healthy");
        health.Timestamp.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
        health.Timestamp.Should().BeAfter(DateTime.UtcNow.AddMinutes(-1));
    }
    
    private record HealthCheckResponse(string Status, DateTime Timestamp);
}