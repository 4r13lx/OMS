namespace Oms.PriceMock.Application.Dtos;

public class Asset
{
    public int Id { get; set; }

    public string Ticker { get; set; } = string.Empty;

    public string Nombre { get; set; } = string.Empty;

    public int TipoActivo { get; set; }

    public decimal PrecioUnitario { get; set; }
}