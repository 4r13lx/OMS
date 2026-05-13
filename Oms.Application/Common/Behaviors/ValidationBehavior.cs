using FluentValidation;
using MediatR;
using Oms.Core.Exceptions;

namespace Oms.Application.Common.Behaviors
{
    /// <summary>
    /// Pipeline Behavior de MediatR para automatizar la validación.
    /// Este es un ejemplo de "Cross-Cutting Concern" (aspecto transversal).
    /// Permite interceptar CUALQUIER comando o consulta antes de que llegue a su Handler.
    /// </summary>
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            // Si no hay validadores registrados para este tipo de petición, continuamos al siguiente paso.
            if (!_validators.Any())
            {
                return await next();
            }

            // Ejecutamos todos los validadores registrados para esta TRequest de forma asíncrona.
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            
            // Recolectamos todos los errores encontrados.
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f != null).ToList();

            if (failures.Count != 0)
            {
                // Al lanzar una excepción aquí, evitamos que el Handler llegue a ejecutarse.
                // Esto garantiza que el Handler solo reciba datos válidos (Principio de Diseño por Contrato).
                throw new DomainException(failures.First().ErrorMessage);
            }

            // Si todo es válido, llamamos a "next()" para continuar con el siguiente comportamiento o el Handler.
            return await next();
        }
    }
}
