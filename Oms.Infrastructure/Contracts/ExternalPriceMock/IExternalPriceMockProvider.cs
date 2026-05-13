using System;

namespace Oms.Infrastructure.Contracts.ExternalPriceMock
{
    /// <summary>
    /// Interfaz para proveedores externos de precios de mercado.
    /// </summary>
    /// <remarks>
    /// Esta interfaz es un ejemplo del principio de Inversión de Dependencia (D en SOLID).
    /// La aplicación depende de una abstracción, permitiendo intercambiar la implementación real
    /// (ej: un servicio de Bloomberg, Reuters o un Mock para pruebas) sin afectar la lógica de negocio.
    /// </remarks>
    public interface IExternalPriceMockProvider
    {
        /// <summary>
        /// Obtiene el precio actual de mercado para un ticker específico de forma asíncrona.
        /// </summary>
        /// <param name="ticker">El símbolo del activo (ej: AAPL).</param>
        /// <returns>El precio de mercado actual.</returns>
        Task<decimal> GetPriceAsync(string ticker);
    }
}
