namespace Oms.Application.Dtos
{
    /// <summary>
    /// Objeto de Transferencia de Datos (DTO) para el resumen consolidado de una cuenta.
    /// </summary>
    /// <remarks>
    /// Este DTO es un ejemplo de "Data Aggregation" (Agregación de Datos). En lugar de enviar
    /// una lista cruda de órdenes, el servicio calcula estadísticas y las agrupa en este objeto,
    /// optimizando el ancho de banda y reduciendo la lógica necesaria en el cliente.
    /// </remarks>
    public sealed class AccountSummaryResponse
    {
        /// <summary> Identificador de la cuenta. </summary>
        public int AccountId { get; set; }

        /// <summary> Cantidad total de órdenes registradas. </summary>
        public int TotalOrders { get; set; }

        /// <summary> Monto nocional total operado (suma de montos brutos). </summary>
        public decimal TotalNotionalAmount { get; set; }

        /// <summary> Suma total de comisiones pagadas. </summary>
        public decimal TotalCommissionAmount { get; set; }

        /// <summary> Suma total de impuestos pagados. </summary>
        public decimal TotalTaxAmount { get; set; }

        /// <summary> 
        /// Desglose de órdenes agrupadas por su estado.
        /// La clave es el nombre del estado (ej: "Ejecutada") y el valor es la cantidad de órdenes.
        /// </summary>
        public IDictionary<string, int> OrdersByStatus { get; set; } = new Dictionary<string, int>();
    }
}
