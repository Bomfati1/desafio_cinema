using Cinema.Api.DTOs;
using Cinema.Api.Validators;
using FluentValidation.TestHelper;
using Xunit;

namespace Cinema.Api.Tests;

public class CreateSessionValidatorTests
{
    private readonly CreateSessionValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_MovieId_Zero()
    {
        var request = new CreateSessionRequest(0, 1, DateTime.UtcNow, DateTime.UtcNow.AddHours(2), 35);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.MovieId);
    }

    [Fact]
    public void Should_Have_Error_When_EndTime_Before_StartTime()
    {
        var request = new CreateSessionRequest(1, 1,
            new DateTime(2026, 8, 1, 17, 0, 0),
            new DateTime(2026, 8, 1, 14, 0, 0),
            35);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.EndTime);
    }

    [Fact]
    public void Should_Have_Error_When_Negative_Price()
    {
        var request = new CreateSessionRequest(1, 1, DateTime.UtcNow, DateTime.UtcNow.AddHours(2), -10);
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.TicketPrice);
    }

    [Fact]
    public void Should_Pass_With_Valid_Request()
    {
        var request = new CreateSessionRequest(1, 1,
            new DateTime(2026, 8, 1, 14, 0, 0),
            new DateTime(2026, 8, 1, 17, 0, 0),
            35);
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}

public class CreateReservationValidatorTests
{
    private readonly CreateReservationValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Name_Empty()
    {
        var request = new CreateReservationRequest(1, "", new List<int> { 1 });
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.CustomerName);
    }

    [Fact]
    public void Should_Have_Error_When_No_Seats()
    {
        var request = new CreateReservationRequest(1, "João", new List<int>());
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SeatIds);
    }

    [Fact]
    public void Should_Have_Error_When_Duplicate_Seats()
    {
        var request = new CreateReservationRequest(1, "João", new List<int> { 1, 1, 2 });
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.SeatIds);
    }
}

public class LoginValidatorTests
{
    private readonly LoginValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_Email_Invalid()
    {
        var request = new LoginRequest("invalid", "123");
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_Pass_With_Valid_Request()
    {
        var request = new LoginRequest("test@email.com", "123456");
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
