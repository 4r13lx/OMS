using System;
using Oms.Infrastructure.Contracts.ExternalPriceMock;
using Oms.Core.Exceptions;
using System.Net.Http.Json;
using Oms.Infrastructure.Dtos.ExternalPriceMock;

namespace Oms.Infrastructure.Services.ExternalPriceMock;

/// <summary>
/// Implementación de infraestructura para obtener precios de un servicio externo.
/// Este servicio se inyecta en la capa de Aplicación a través de una interfaz,
/// manteniendo el dominio independiente de los detalles de red (Principios de Clean Architecture).
/// </summary>
public sealed class ExternalPriceMockProvider : IExternalPriceMockProvider
{
    private readonly HttpClient _httpClient;

    public ExternalPriceMockProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Obtiene el precio de mercado para un ticker dado.
    /// Nota: Este método se beneficia de la política de reintentos configurada en Program.cs.
    /// </summary>
    public async Task<decimal> GetPriceAsync(string ticker)
    {
        var response = await _httpClient.GetAsync($"api/prices/{ticker}");
        
        // Manejo de errores específicos del dominio
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            throw new DomainException($"Precio no encontrado para el ticker '{ticker}'.");
        }

        // Si falla después de los reintentos de Polly, lanzamos una excepción de servicio externo.
        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalServiceException("No se pudo obtener el precio del servicio externo de mercado.");
        }

        var content = await response.Content.ReadFromJsonAsync<AssetResponse>();
        if (content is null)
        {
            throw new ExternalServiceException("Respuesta inválida del servicio de precios externos.");
        }

        return content.PrecioUnitario;
    }
}