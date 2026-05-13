using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Oms.PriceMock.Application.Exceptions;

namespace Oms.PriceMock.Api.Infrastructure;

/// <summary>
/// Manejador global de excepciones para el simulador de precios.
/// Al igual que en la API principal, este componente centraliza la captura de errores para
/// devolver respuestas consistentes bajo el estándar RFC 7807 (Problem Details).
/// Esto asegura que los clientes del simulador reciban información clara incluso cuando
/// el simulador falla deliberadamente para probar la resiliencia.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Gestiona las excepciones ocurridas en el simulador, mapeándolas a respuestas HTTP estandarizadas.
    /// </summary>
    /// <param name="httpContext">Contexto HTTP de la solicitud.</param>
    /// <param name="exception">Excepción producida.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>True si la excepción fue procesada.</returns>
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, exception.Message);

        var response = exception switch
        {
            UnauthorizedAccessException => new ProblemDetails
            {
                Title = "Unauthorized",
                Detail = "No autorizado",
                Status = StatusCodes.Status401Unauthorized
            },
            KeyNotFoundException => new ProblemDetails
            {
                Title = "Not Found",
                Detail = "Recurso no encontrado",
                Status = StatusCodes.Status404NotFound
            },
            ArgumentException => new ProblemDetails
            {
                Title = "Bad Request",
                Detail = "Solicitud inválida",
                Status = StatusCodes.Status400BadRequest
            },
            ServiceUnavailableException => new ProblemDetails
            {
                Title = "External Service Error",
                Detail = exception.Message,
                Status = StatusCodes.Status503ServiceUnavailable
            },
            _ => new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "Ocurrió un error inesperado.",
                Status = StatusCodes.Status500InternalServerError
            }
        };

        httpContext.Response.StatusCode = response.Status ?? 500;

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true;
    }
}