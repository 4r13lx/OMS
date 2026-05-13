using Oms.PriceMock.Application.Dtos;
using Oms.PriceMock.Application.Contracts;

namespace Oms.PriceMock.Application.Simulation.NetworkIssues;

/// <summary>
/// Implementación del patrón de diseño Decorator para el servicio de simulación de precios.
/// El patrón Decorator permite añadir funcionalidades a un objeto de forma dinámica sin modificar su estructura original.
/// En este caso, este decorador añade capacidades de "Rate Limiting" (limitación de tasa) y simulación de disponibilidad
/// sobre cualquier implementación de <see cref="IPriceMockService"/>.
/// </summary>
/// <remarks>
/// Beneficios de usar este patrón:
/// 1. Principio de Responsabilidad Única: La lógica de límite de tasa está separada de la lógica de obtención de precios.
/// 2. Principio Abierto/Cerrado: Podemos extender el comportamiento del servicio sin alterar el código existente.
/// 3. Composición: Se pueden encadenar múltiples decoradores para añadir diversas capas de funcionalidad.
/// </remarks>
public class RateLimitDecorator : IPriceMockService
{
    private readonly IPriceMockService _innerService;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="RateLimitDecorator"/>.
    /// </summary>
    /// <param name="innerService">La instancia del servicio original que será decorada.</param>
    public RateLimitDecorator(IPriceMockService innerService)
    {
        _innerService = innerService;
    }

    /// <summary>
    /// Obtiene la lista de activos financieros, aplicando simulaciones de latencia y disponibilidad antes de delegar al servicio interno.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>Una colección de activos financieros.</returns>
    public async Task<IEnumerable<Asset>> GetAssetsAsync(CancellationToken cancellationToken = default)
    {
        // El decorador intercepta la llamada para añadir funcionalidad adicional.
        //await RateLimitSimulator.SimulateAsync(cancellationToken);
        //await RateLimitSimulator.AvailabilityCheckAsync(cancellationToken);
        
        // Delegación al objeto envuelto (wrapped object).
        return await _innerService.GetAssetsAsync(cancellationToken);
    }

    /// <summary>
    /// Obtiene un activo financiero por su ticker, aplicando simulaciones de restricciones antes de delegar al servicio interno.
    /// </summary>
    /// <param name="ticker">El símbolo del activo.</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>El activo financiero encontrado.</returns>
    public async Task<Asset> GetAssetsByTickerAsync(string ticker, CancellationToken cancellationToken = default)
    {
        // Se añade comportamiento antes de la ejecución del método original.
        //await RateLimitSimulator.SimulateAsync(cancellationToken);
        //await RateLimitSimulator.AvailabilityCheckAsync(cancellationToken);
        
        return await _innerService.GetAssetsByTickerAsync(ticker, cancellationToken);
    }
}