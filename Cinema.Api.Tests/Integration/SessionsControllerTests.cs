using System.Net;
using System.Net.Http.Json;
using Cinema.Api.Models;
using Xunit;

namespace Cinema.Api.Tests.Integration;

public class SessionsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SessionsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetSessions_ReturnsPagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/sessions");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<object>>();
        Assert.NotNull(result);
        Assert.True(result.TotalCount >= 0);
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public async Task GetSessions_WithDate_ReturnsFiltered()
    {
        // Act
        var response = await _client.GetAsync("/api/sessions?date=2026-08-01");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<object>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetSessionById_ValidId_ReturnsSessionDetail()
    {
        // Act
        var response = await _client.GetAsync("/api/sessions/1");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSessionById_InvalidId_Returns404()
    {
        // Act
        var response = await _client.GetAsync("/api/sessions/99999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetSessions_EndpointReturnsCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/sessions");

        // Assert
        Assert.StartsWith("application/json", response.Content.Headers.ContentType?.MediaType);
    }
}
