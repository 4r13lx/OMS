using Oms.Core.Enums;

namespace Oms.Core.Entities
{
    /// <summary>
    /// Entidad de Dominio que representa una Orden de Inversión.
    /// En Clean Architecture, las entidades contienen el estado y las reglas críticas del negocio.
    /// </summary>
    public sealed class InvestmentOrder
    {
        // Propiedades de estado de la orden
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string Ticker { get; set; } = null!;
        public string AssetName { get; set; } = null!;
        public AssetType AssetType { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public char Operation { get; set; } // 'C' para Compra, 'V' para Venta
        
        // El estado (Status) tiene un setter privado para asegurar que solo se modifique
        // a través de métodos controlados, respetando el encapsulamiento.
        public OrderStatus Status { get; private set; }
        
        // Valores calculados por la lógica de negocio
        public decimal TotalAmount { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Constructor que inicializa la orden en su estado inicial "En Proceso".
        /// Esto garantiza que ninguna orden nazca en un estado inválido.
        /// </summary>
        public InvestmentOrder()
        {
            Status = OrderStatus.EnProceso;
            CreatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Método de dominio para gestionar la transición de estados.
        /// Aquí se podrían añadir reglas de validación adicionales (ej. no pasar de Ejecutada a EnProceso).
        /// </summary>
        public void SetStatus(OrderStatus status)
        {
            if (Status == status)
            {
                return;
            }

            Status = status;
        }
    }
}
