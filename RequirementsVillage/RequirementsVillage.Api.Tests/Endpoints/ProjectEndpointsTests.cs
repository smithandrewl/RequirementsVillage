namespace RequirementsVillage.Api.Tests.Endpoints;

public class ProjectEndpointsTests : IClassFixture<TestWebApplicationFactory<Program>> {
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProjectEndpointsTests(TestWebApplicationFactory<Program> factory) {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetProjects_ReturnsOkResult_WithListOfProjects() {
        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var projects = await response.Content.ReadFromJsonAsync<List<Project>>();
        projects.Should().NotBeNull();
        projects.Should().HaveCount(2);
        
        (projects is [{ Name: "Task Management System" }, _]).Should().BeTrue();
        (projects is [_, { Name: "Weather Widget" }]).Should().BeTrue();
    }

    [Fact]
    public async Task GetProjects_ReturnsProjectsWithExpectedProperties() {
        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var projects = await response.Content.ReadFromJsonAsync<List<Project>>();
        projects.Should().NotBeNull();
        
        var firstProject = projects![0];
        firstProject.Id.Should().NotBeEmpty();
        firstProject.Name.Should().NotBeNullOrWhiteSpace();
        firstProject.Description.Should().NotBeNullOrWhiteSpace();
        firstProject.Category.Should().NotBeNullOrWhiteSpace();
        firstProject.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        firstProject.UpdatedAt.Should().BeBefore(DateTime.UtcNow);
    }
}