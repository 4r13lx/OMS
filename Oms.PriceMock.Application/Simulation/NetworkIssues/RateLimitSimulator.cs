using Oms.PriceMock.Application.Exceptions;

namespace Oms.PriceMock.Application.Simulation.NetworkIssues;

/// <summary>
/// Proporciona utilidades para simular restricciones de red y disponibilidad de servicios externos.
/// Esta clase es fundamental para realizar pruebas de resiliencia (resilience testing) y verificar cómo se comporta el sistema bajo condiciones adversas.
/// </summary>
/// <remarks>
/// Beneficios de simular restricciones:
/// 1. Pruebas de Timeouts: Permite verificar que el sistema maneja correctamente las esperas prolongadas.
/// 2. Manejo de Errores: Simula caídas de servicio para asegurar que los bloques try-catch y las políticas de reintento funcionen.
/// 3. Determinismo en Pruebas: Aunque usa aleatoriedad, permite recrear escenarios de "peor caso" de forma controlada.
/// </remarks>
public class RateLimitSimulator
{
    /// <summary>
    /// Simula de forma aleatoria una latencia significativa en la respuesta del servicio.
    /// Introduce un retraso de 10 segundos con una probabilidad del 15%.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación para interrumpir la espera si es necesario.</param>
    public static async Task SimulateAsync(CancellationToken cancellationToken = default)
    {
        if (Random.Shared.NextDouble() < 0.15) // 15% de probabilidad de no estar disponible
        {
            var delay = TimeSpan.FromSeconds(10);
            await Task.Delay(delay, cancellationToken);
        }   
    }

    /// <summary>
    /// Verifica la disponibilidad simulada del servicio, lanzando una excepción si el servicio se considera "caído".
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación para la operación.</param>
    /// <exception cref="ExternalServiceException">Se lanza con una probabilidad del 15% para simular un fallo en el servicio externo.</exception>
    public static async Task AvailabilityCheckAsync(CancellationToken cancellationToken = default)
    {
        if (Random.Shared.NextDouble() < 0.15) // 15% de probabilidad de no estar disponible
        {
            throw new ServiceUnavailableException("La API externa no está disponible en este momento.");
        }
        await Task.CompletedTask;
    }
}