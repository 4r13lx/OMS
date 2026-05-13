namespace Oms.PriceMock.Application.Exceptions;

/// <summary>
/// Excepción lanzada cuando ocurre un error en la comunicación o procesamiento con un servicio externo.
/// </summary>
/// <remarks>
/// Esta excepción es útil para envolver fallos técnicos de servicios externos y presentarlos
/// de manera controlada al dominio, permitiendo decidir si el proceso debe reintentarse o fallar.
/// </remarks>
public class ServiceUnavailableException : Exception
{
    public ServiceUnavailableException(string message) : base(message)
    {
    }
}
