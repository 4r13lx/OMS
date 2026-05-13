using System;
using System.Text.Json.Serialization;

namespace Oms.Infrastructure.Dtos.ExternalPriceMock
{
    public class AssetResponse
    {
        // Propiedades que coinciden con la respuesta del servicio externo
        // Si los nombres de las propiedades en el JSON coinciden exactamente con los nombres de las propiedades en la clase, no es necesario usar JsonPropertyName. Sin embargo, se incluyen aquí para mayor claridad y para evitar problemas si los nombres cambian en el futuro.
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("ticker")]
        public string Ticker { get; set; } = string.Empty;

        [JsonPropertyName("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [JsonPropertyName("tipoActivo")]
        public int TipoActivo { get; set; }

        [JsonPropertyName("precioUnitario")]
        public decimal PrecioUnitario { get; set; }
    }   
}
