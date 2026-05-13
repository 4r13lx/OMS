using Oms.Core.Enums;

namespace Oms.Application.Dtos
{
    /// <summary>
    /// Objeto de Transferencia de Datos (DTO) para la actualización de una orden existente.
    /// </summary>
    /// <remarks>
    /// En operaciones de actualización (PUT/PATCH), es una buena práctica usar DTOs específicos.
    /// Esto permite restringir qué propiedades pueden ser modificadas por el usuario, evitando 
    /// que se alteren campos críticos como el ID o la fecha de creación de forma accidental.
    /// </remarks>
    public sealed class OrderUpdateRequest
    {
        /// <summary> Nuevo estado que se desea asignar a la orden. </summary>
        public OrderStatus Status { get; set; }
    }
}
