using Oms.PriceMock.Application.Dtos;
using Oms.PriceMock.Application.Contracts;

namespace Oms.PriceMock.Application.Services.Services;

/// <summary>
/// Implementación del servicio de simulación de precios externos.
/// Este servicio actúa como un intermediario que recupera datos de activos financieros y simula actualizaciones de precios.
/// </summary>
public class PriceMockService : IPriceMockService
{
    private readonly HttpClient _httpClient;
    private readonly IAssetStore _assetStore;

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="PriceMockService"/>.
    /// </summary>
    /// <param name="httpClient">Cliente HTTP para posibles llamadas externas (aunque aquí se usa principalmente el store local).</param>
    /// <param name="assetStore">El almacén de activos que contiene los datos en memoria.</param>
    public PriceMockService(HttpClient httpClient, IAssetStore assetStore)
    {
        _httpClient = httpClient;
        _assetStore = assetStore;
    }

    /// <summary>
    /// Recupera de forma asíncrona todos los activos financieros disponibles.
    /// </summary>
    /// <param name="cancellationToken">Token de cancelación para la operación.</param>
    /// <returns>Una lista de activos financieros.</returns>
    public async Task<IEnumerable<Asset>> GetAssetsAsync(CancellationToken cancellationToken = default)
    {
        return await _assetStore.GetAllAsync();
    }

    /// <summary>
    /// Obtiene un activo financiero específico por su ticker y calcula su precio actual simulando variaciones del mercado.
    /// </summary>
    /// <param name="ticker">El símbolo del activo (ej: AAPL, BTC).</param>
    /// <param name="cancellationToken">Token de cancelación para la operación.</param>
    /// <returns>El activo financiero con su precio actualizado, o un nuevo objeto si no se encuentra en el store.</returns>
    public async Task<Asset> GetAssetsByTickerAsync(string ticker, CancellationToken cancellationToken = default)
    {
        var result = default(Asset);
        var currentPrice = default(decimal);

        if (!_assetStore.TryGetValue(ticker, out var basePrice))
        {
            result = null; // Simula ticker no encontrado
        }
        else
        {
            currentPrice = await _assetStore.CalculateVariationAsync(ticker, basePrice);
            result = await Task.FromResult(_assetStore.GetAllAsync().Result.FirstOrDefault(x=>x.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase)));            
        }

        return result ?? new Asset();
    }
}