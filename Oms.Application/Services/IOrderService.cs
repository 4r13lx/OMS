using Oms.Application.Dtos;

namespace Oms.Application.Services
{
    /// <summary>
    /// Interfaz que define las operaciones de negocio para la gestión de órdenes.
    /// </summary>
    /// <remarks>
    /// Actualmente, este servicio forma parte de una **Arquitectura en Capas** tradicional.
    /// Sin embargo, el proyecto está evolucionando hacia **CQRS** (Command Query Responsibility Segregation).
    /// Esta interfaz actúa como un punto intermedio, centralizando la lógica que eventualmente será 
    /// distribuida en comandos y consultas independientes.
    /// </remarks>
    public interface IOrderService
    {
        /// <summary> Crea una nueva orden de inversión procesando reglas de negocio, comisiones e impuestos. </summary>
        Task<OrderResponse> CreateOrderAsync(OrderCreateRequest request);

        /// <summary> Recupera todas las órdenes registradas en el sistema. </summary>
        Task<IReadOnlyList<OrderResponse>> GetOrdersAsync();

        /// <summary> Busca una orden específica por su identificador único. </summary>
        Task<OrderResponse> GetOrderByIdAsync(int id);

        /// <summary> Actualiza el estado de una orden (ej: de Iniciada a Ejecutada). </summary>
        Task UpdateOrderStatusAsync(int id, OrderUpdateRequest request);

        /// <summary> Elimina una orden del sistema. </summary>
        Task DeleteOrderAsync(int id);

        /// <summary> Genera un resumen estadístico de las operaciones de una cuenta. </summary>
        Task<AccountSummaryResponse> GetAccountSummaryAsync(int accountId);
    }
}
