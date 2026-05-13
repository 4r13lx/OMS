using MediatR;
using Oms.Application.Dtos;

namespace Oms.Application.Orders.Commands
{
    /// <summary>
    /// Comando para la creación de una nueva orden de inversión.
    /// </summary>
    /// <remarks>
    /// Dentro del patrón CQRS (Command Query Responsibility Segregation), un **Command** representa 
    /// una intención de cambiar el estado del sistema. 
    /// A diferencia de las Queries, los Commands:
    /// 1. Pueden modificar datos.
    /// 2. Generalmente no devuelven datos, aunque en APIs REST es común devolver el recurso creado o su ID.
    /// MediatR se utiliza para desacoplar el emisor del comando de su manejador (Handler).
    /// </remarks>
    public sealed record CreateOrderCommand(OrderCreateRequest Request) : IRequest<OrderResponse>;
}
