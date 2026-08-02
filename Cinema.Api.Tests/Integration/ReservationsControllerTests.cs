using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Cinema.Api.DTOs;
using Xunit;

namespace Cinema.Api.Tests.Integration;

public class ReservationsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReservationsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateReservation_NoToken_Returns401()
    {
        // Arrange
        var request = new CreateReservationRequest(1, "João", new List<int> { 1 });

        // Act
        var response = await _client.PostAsJsonAsync("/api/reservations", request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateReservation_SessionNotFound_Returns404()
    {
        // Arrange — obtém token válido primeiro
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin@cinema.com", "admin"));

        if (loginResponse.StatusCode == HttpStatusCode.OK)
        {
            var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", login!.Token);

            var request = new CreateReservationRequest(99999, "João", new List<int> { 1 });

            // Act
            var response = await _client.PostAsJsonAsync("/api/reservations", request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }

    [Fact]
    public async Task CreateReservation_InvalidData_Returns400()
    {
        // Arrange — tenta criar com payload inválido
        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest("admin@cinema.com", "admin"));

        if (loginResponse.StatusCode == HttpStatusCode.OK)
        {
            var login = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();
            _client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", login!.Token);

            var request = new { sessionId = 0, customerName = "", seatIds = new List<int>() };

            // Act
            var response = await _client.PostAsJsonAsync("/api/reservations", request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
