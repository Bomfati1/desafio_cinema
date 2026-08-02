using System.Net;
using System.Text.Json;
using Cinema.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Api.Middleware;

public class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Domain error: {Message}", ex.Message);
            await WriteProblemDetails(context, ex.StatusCode, ex.Message);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            _logger.LogWarning(ex, "Concurrency conflict (unique constraint violation)");
            await WriteProblemDetails(context,
                (int)HttpStatusCode.Conflict,
                "Conflito de reserva: um ou mais assentos já foram reservados por outro usuário.",
                "https://tools.ietf.org/html/rfc7807");
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Database update error: {Message}", ex.Message);
            await WriteProblemDetails(context, (int)HttpStatusCode.InternalServerError,
                "Erro interno do servidor.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error: {Message}", ex.Message);
            await WriteProblemDetails(context, (int)HttpStatusCode.InternalServerError,
                "Erro interno do servidor.");
        }
    }

    private static async Task WriteProblemDetails(HttpContext context, int statusCode,
        string detail, string? type = null)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var problem = new ProblemDetails
        {
            Type = type ?? "https://tools.ietf.org/html/rfc7807",
            Title = statusCode switch
            {
                400 => "Requisição inválida",
                401 => "Não autorizado",
                404 => "Recurso não encontrado",
                409 => "Conflito",
                _ => "Erro"
            },
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await context.Response.WriteAsync(json);
    }

    /// <summary>
    /// Detecta violação de unique constraint (ex: double-booking de assento).
    /// Funciona com PostgreSQL (Npgsql), SQL Server e SQLite.
    /// </summary>
    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        var message = ex.InnerException?.Message ?? string.Empty;
        return message.Contains("23505") ||
               message.Contains("unique") ||
               message.Contains("UNIQUE constraint");
    }
}
