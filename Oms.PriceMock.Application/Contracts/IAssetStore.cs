using Oms.PriceMock.Application.Dtos;

namespace Oms.PriceMock.Application.Contracts;

/// <summary>
/// Define la interfaz para el almacenamiento de activos financieros.
/// Proporciona métodos para gestionar el acceso y la actualización de precios en memoria.
/// </summary>
public interface IAssetStore
{
    /// <summary>
    /// Calcula y aplica una variación al precio de un activo.
    /// </summary>
    /// <param name="ticker">El símbolo del activo.</param>
    /// <param name="basePrice">El precio base inicial.</param>
    /// <returns>El nuevo precio calculado.</returns>
    Task<decimal> CalculateVariationAsync(string ticker, decimal basePrice);

    /// <summary>
    /// Obtiene todos los activos financieros del almacén.
    /// </summary>
    /// <returns>Una colección de activos financieros.</returns>
    Task<IEnumerable<Asset>> GetAllAsync();

    /// <summary>
    /// Intenta obtener el precio de un activo por su ticker.
    /// </summary>
    /// <param name="ticker">El símbolo del activo.</param>
    /// <param name="price">El precio devuelto.</param>
    /// <returns>Verdadero si se encontró el activo, falso en caso contrario.</returns>
    bool TryGetValue(string ticker, out decimal price);

    /// <summary>
    /// Actualiza el precio de un activo específico de forma asíncrona.
    /// </summary>
    /// <param name="ticker">El símbolo del activo.</param>
    /// <param name="price">El nuevo precio.</param>
    Task UpdatePriceAsync(string ticker, decimal price);
}