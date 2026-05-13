using FluentValidation;
using Oms.Application.Dtos;

namespace Oms.Application.Validators
{
    /// <summary>
    /// Validador para la solicitud de creación de órdenes.
    /// </summary>
    /// <remarks>
    /// Utilizamos FluentValidation para implementar la validación de forma declarativa.
    /// Esta aproximación permite:
    /// 1. Separar la lógica de validación del modelo de datos y de la lógica de negocio del servicio.
    /// 2. Crear reglas complejas de forma legible (Fluent Interface).
    /// 3. Centralizar las reglas de validación en un solo lugar, facilitando el mantenimiento y las pruebas unitarias.
    /// </remarks>
    public sealed class OrderCreateRequestValidator : AbstractValidator<OrderCreateRequest>
    {
        public OrderCreateRequestValidator()
        {
            // Regla: El ID de cuenta es fundamental para la trazabilidad y seguridad.
            RuleFor(x => x.AccountId)
                .GreaterThan(0)
                .WithMessage("El ID de la cuenta es obligatorio y debe ser mayor que cero.");

            // Regla: El Ticker identifica unívocamente al activo en el mercado.
            RuleFor(x => x.Ticker)
                .NotEmpty()
                .WithMessage("El ticker del activo es obligatorio.");

            // Regla: Validación de longitud para evitar ataques de desbordamiento o datos basura.
            // RuleFor(x => x.AssetName)
            //     .NotEmpty()
            //     .WithMessage("El nombre del activo es obligatorio.")
            //     .MaximumLength(32)
            //     .WithMessage("El nombre del activo debe tener como máximo 32 caracteres.");

            // Regla: No se permiten operaciones con cantidad cero o negativa.
            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("La cantidad debe ser mayor que cero.");

            // Regla personalizada: Solo permitimos 'C' o 'V'.
            RuleFor(x => x.Operation)
                .Must(x => char.ToUpperInvariant(x) == 'C' || char.ToUpperInvariant(x) == 'V')
                .WithMessage("La operación debe ser 'C' para Compra o 'V' para Venta.");
        }
    }
}
