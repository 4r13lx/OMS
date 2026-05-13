namespace Oms.Core.Exceptions
{
    /// <summary>
    /// Representa una excepción base para todas las violaciones de reglas de negocio en el dominio.
    /// </summary>
    /// <remarks>
    /// El uso de excepciones de dominio permite que la capa de aplicación capture errores específicos 
    /// de la lógica de negocio y los traduzca a respuestas HTTP adecuadas (como un 400 Bad Request).
    /// Esto separa las preocupaciones de negocio de las excepciones técnicas de infraestructura.
    /// </remarks>
    public class DomainException : Exception
    {
        public DomainException(string message) : base(message)
        {
            // El mensaje se pasa a la clase base Exception para su manejo estándar.
        }
    }
}
