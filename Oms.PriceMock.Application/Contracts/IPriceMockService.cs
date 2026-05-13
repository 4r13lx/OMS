using Oms.PriceMock.Application.Dtos;

namespace Oms.PriceMock.Application.Contracts;

/// <summary>
/// Define el contrato para el servicio de simulación de precios externos.
/// Este servicio permite consultar activos y sus precios actualizados.
/// </summary>
public interface IPriceMockService
{
    /// <summary>
    /// Obtiene una lista de todos los activos financieros disponibles de forma asíncrona.
    /// </summary>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>Una colección de activos financieros.</returns>
    Task<IEnumerable<Asset>> GetAssetsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la información detallada de un activo financiero específico mediante su ticker.
    /// </summary>
    /// <param name="ticker">El símbolo identificador del activo.</param>
    /// <param name="cancellationToken">Token para cancelar la operación.</param>
    /// <returns>El activo financiero solicitado.</returns>
    Task<Asset> GetAssetsByTickerAsync(string ticker, CancellationToken cancellationToken = default);
}
