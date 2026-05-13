using Oms.Core.Enums;

namespace Oms.Application.Dtos
{
    /// <summary>
    /// Objeto de Transferencia de Datos (DTO) que representa la respuesta detallada de una orden.
    /// </summary>
    /// <remarks>
    /// Este DTO consolida la información del dominio y los cálculos de negocio (comisiones, impuestos)
    /// para ser consumidos por el cliente (UI o API externa). 
    /// Al igual que los Requests, protege la integridad del dominio al no exponer directamente la entidad de base de datos.
    /// </remarks>
    public sealed class OrderResponse
    {
        /// <summary> Identificador único de la orden generado por el sistema. </summary>
        public int Id { get; set; }

        /// <summary> Cuenta asociada a la orden. </summary>
        public int AccountId { get; set; }

        /// <summary> Ticker del activo financiero. </summary>
        public string Ticker { get; set; } = null!;

        /// <summary> Nombre del activo financiero. </summary>
        public string AssetName { get; set; } = null!;

        /// <summary> Tipo de activo (Acción, Bono, FCI). </summary>
        public AssetType AssetType { get; set; }

        /// <summary> Cantidad operada. </summary>
        public int Quantity { get; set; }

        /// <summary> Precio unitario de ejecución. </summary>
        public decimal Price { get; set; }

        /// <summary> Operación realizada ('C'/'V'). </summary>
        public char Operation { get; set; }

        /// <summary> Estado actual de la orden (Iniciada, Ejecutada, etc.). </summary>
        public OrderStatus Status { get; set; }

        /// <summary> Monto total bruto de la operación (Precio * Cantidad). </summary>
        public decimal TotalAmount { get; set; }

        /// <summary> Comisión calculada por el broker. </summary>
        public decimal CommissionAmount { get; set; }

        /// <summary> Impuestos asociados (ej: IVA sobre comisiones). </summary>
        public decimal TaxAmount { get; set; }

        /// <summary> Fecha y hora de creación de la orden. </summary>
        public DateTime CreatedAt { get; set; }
    }
}
