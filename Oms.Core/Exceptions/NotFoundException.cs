namespace Oms.Core.Exceptions
{
    /// <summary>
    /// Excepción lanzada cuando no se encuentra un recurso solicitado en el dominio.
    /// </summary>
    /// <remarks>
    /// Al heredar de <see cref="DomainException"/>, permite un manejo centralizado donde
    /// la capa de API puede capturar esta excepción específicamente y retornar un código 404 (Not Found).
    /// </remarks>
    public class NotFoundException : DomainException
    {
        public NotFoundException(string message) : base(message)
        {
        }
    }
}
