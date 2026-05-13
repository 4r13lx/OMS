namespace Oms.Application.Dtos
{
    /// <summary>
    /// Objeto de Transferencia de Datos (DTO) para la creación de una orden.
    /// </summary>
    /// <remarks>
    /// El uso de DTOs es una decisión arquitectónica clave para desacoplar la API (capa externa) 
    /// de los modelos de Dominio. Esto permite:
    /// 1. Seguridad: No exponer campos internos del modelo de datos que no deben ser modificados por el usuario.
    /// 2. Flexibilidad: Cambiar el modelo de dominio sin romper los contratos de la API.
    /// 3. Optimización: Transferir solo la información estrictamente necesaria.
    /// </remarks>
    public sealed class OrderCreateRequest
    {
        /// <summary> Identificador de la cuenta que realiza la operación. </summary>
        public int AccountId { get; set; }

        /// <summary> Símbolo o Ticker del activo financiero (ej: AAPL, GGAL). </summary>
        public required string Ticker { get; set; } = null!;

        /// <summary> Nombre descriptivo del activo. </summary>
        //public string AssetName { get; set; } = null!;

        /// <summary> Cantidad de activos a operar. </summary>
        public required int Quantity { get; set; }

        /// <summary> 
        /// Precio unitario de la operación. 
        /// Es opcional ya que para ciertos activos el precio se obtiene de un servicio externo.
        /// </summary>
        public decimal? Price { get; set; }

        /// <summary> Tipo de operación: 'C' para Compra, 'V' para Venta. </summary>
        public required char Operation { get; set; }
    }
}
