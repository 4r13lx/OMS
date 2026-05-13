using System.Text.Json;
using Oms.PriceMock.Application.Dtos;
using Oms.PriceMock.Application.Contracts;

namespace Oms.PriceMock.Application.Repositories;

/// <summary>
/// Representa un almacenamiento en memoria (In-Memory Store) para los activos financieros y sus precios.
/// Esta clase actúa como una base de datos local temporal, permitiendo un acceso rápido y manipulación de datos sin la necesidad de una base de datos externa persistente.
/// </summary>
/// <remarks>
/// El uso de un "In-Memory Store" es ideal para:
/// 1. Mocking/Simulación: Proporcionar datos de prueba consistentes durante el desarrollo.
/// 2. Rendimiento: Acceso instantáneo a los datos sin latencia de red o disco.
/// 3. Simplicidad: Facilita la implementación de prototipos y pruebas unitarias.
/// En este contexto, carga los datos iniciales desde un archivo JSON y mantiene el estado de los precios en memoria durante el ciclo de vida de la aplicación.
/// </remarks>
public class AssetStore : IAssetStore
{

    private readonly IEnumerable<Asset> _assets;

    /// <summary>
    /// Inicializa el almacén cargando los activos desde la fuente de datos predefinida.
    /// </summary>
    public AssetStore()
    {
        _assets = Load();
    }

    /// <summary>
    /// Carga los activos financieros procesando los datos crudos del JSON al modelo de dominio.
    /// </summary>
    /// <returns>Una colección de activos financieros cargados.</returns>
    private IEnumerable<Asset> Load()
    {
        return ReadPricesFromJson();
    }

    /// <summary>
    /// Lee los datos de los activos desde un archivo JSON local.
    /// </summary>
    /// <returns>Una colección de DTOs de activos.</returns>
    /// <exception cref="FileNotFoundException">Se lanza si el archivo de configuración de activos no existe.</exception>
    private IEnumerable<Asset> ReadPricesFromJson()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, "Assets", "assets.json");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("No se encontró el archivo de activos.", filePath);
        }

        var json = File.ReadAllText(filePath);
        var assets = JsonSerializer.Deserialize<List<Asset>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? [];

        return assets;
    }

    /// <summary>
    /// Recupera todos los activos financieros almacenados en memoria.
    /// </summary>
    /// <returns>Una colección de todos los activos disponibles.</returns>
    public async Task<IEnumerable<Asset>> GetAllAsync()
    {
        return await Task.FromResult(_assets);
    }

    /// <summary>
    /// Intenta obtener el precio base de un activo específico por su ticker.
    /// </summary>
    /// <param name="ticker">El símbolo del activo a buscar.</param>
    /// <param name="price">El precio encontrado (o 0 si no existe).</param>
    /// <returns>Verdadero si el activo existe; de lo contrario, falso.</returns>
    public bool TryGetValue(string ticker, out decimal price)
    {
        var asset = _assets.FirstOrDefault(a => a.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase));
        if (asset != null)
        {
            price = asset.PrecioUnitario;
            return true;
        }

        price = 0;
        return false;
    }

    /// <summary>
    /// Actualiza el precio de un activo financiero en el almacenamiento en memoria.
    /// </summary>
    /// <param name="ticker">El símbolo del activo.</param>
    /// <param name="price">El nuevo precio a establecer.</param>
    public async Task UpdatePriceAsync(string ticker, decimal price)
    {
        var asset = await Task.FromResult(_assets.FirstOrDefault(a => a.Ticker.Equals(ticker, StringComparison.OrdinalIgnoreCase)));
        if (asset != null)
        {
            asset.PrecioUnitario = price;
        }
    }

    /// <summary>
    /// Calcula una variación aleatoria del precio para simular el comportamiento del mercado en tiempo real.
    /// </summary>
    /// <param name="ticker">El símbolo del activo.</param>
    /// <param name="basePrice">El precio base sobre el cual aplicar la variación.</param>
    /// <returns>El nuevo precio calculado tras la variación.</returns>
    public async Task<decimal> CalculateVariationAsync(string ticker, decimal basePrice)
    {
        var variation = (decimal)(Random.Shared.NextDouble() * 0.02 - 0.01);
        var currentPrice = decimal.Round(basePrice * (1 + variation), 4);
        await this.UpdatePriceAsync(ticker, currentPrice);
        return currentPrice;
    }
}