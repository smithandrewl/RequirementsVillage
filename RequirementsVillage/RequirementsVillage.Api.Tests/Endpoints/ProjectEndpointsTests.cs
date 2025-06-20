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
        
        var projectsList = await response.Content.ReadFromJsonAsync<List<Project>>();
        projectsList.Should().NotBeNull();
        
        var projects = toSeq(projectsList!);
        projects.Count.Should().Be(2);
        
        projects.Head.Name.Should().Be("Task Management System");
        projects.Skip(1).Head.Name.Should().Be("Weather Widget");
    }

    [Fact]
    public async Task GetProjects_ReturnsProjectsWithExpectedProperties() {
        // Act
        var response = await _client.GetAsync("/api/projects");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var projectsList = await response.Content.ReadFromJsonAsync<List<Project>>();
        projectsList.Should().NotBeNull();
        
        var projects = toSeq(projectsList!);
        projects.HeadOrNone().Match(
            Some: project => {
                project.Id.Should().NotBeEmpty();
                project.Name.Should().NotBeNullOrWhiteSpace();
                project.Description.Should().NotBeNullOrWhiteSpace();
                project.Category.Should().NotBeNullOrWhiteSpace();
                project.CreatedAt.Should().BeBefore(DateTime.UtcNow);
                project.UpdatedAt.Should().BeBefore(DateTime.UtcNow);
            },
            None: () => throw new InvalidOperationException("Expected at least one project")
        );
    }
}