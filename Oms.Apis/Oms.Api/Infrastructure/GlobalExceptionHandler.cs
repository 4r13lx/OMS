using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Oms.Core.Exceptions;

namespace Oms.Api.Infrastructure;

/// <summary>
/// Implementación de un manejador de excepciones global (Global Exception Handler).
/// Esta clase centraliza la gestión de errores en toda la aplicación, evitando bloques try-catch redundantes
/// y garantizando que todas las respuestas de error sigan un formato consistente.
/// Sigue el estándar 'Problem Details for HTTP APIs' (RFC 7807) para proporcionar información
/// estructurada y legible sobre los errores.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Intenta manejar una excepción ocurrida durante el ciclo de vida de la solicitud HTTP.
    /// Mapea diferentes tipos de excepciones a códigos de estado HTTP específicos y
    /// devuelve una respuesta estructurada utilizando ProblemDetails.
    /// </summary>
    /// <param name="httpContext">Contexto de la solicitud actual.</param>
    /// <param name="exception">La excepción capturada.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>True si la excepción fue manejada con éxito.</returns>
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
            ExternalServiceException => new ProblemDetails
            {
                Title = "External Service Error",
                Detail = exception.Message,
                Status = StatusCodes.Status503ServiceUnavailable
            },
            NotFoundException => new ProblemDetails
            {
                Title = "Not Found",
                Detail = exception.Message,
                Status = StatusCodes.Status404NotFound
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