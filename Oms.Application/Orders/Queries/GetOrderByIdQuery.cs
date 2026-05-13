using MediatR;
using Oms.Application.Dtos;

namespace Oms.Application.Orders.Queries
{
    /// <summary>
    /// Consulta (Query) para obtener una orden por su identificador.
    /// En CQRS, las consultas son objetos inmutables que representan una petición de lectura.
    /// </summary>
    /// <param name="Id">ID único de la orden.</param>
    public sealed record GetOrderByIdQuery(int Id) : IRequest<OrderResponse>;
}
