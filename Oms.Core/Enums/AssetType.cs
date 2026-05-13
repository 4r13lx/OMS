namespace Oms.Core.Enums
{
    /// <summary>
    /// Define los tipos de activos financieros soportados por el sistema.
    /// </summary>
    public enum AssetType
    {
        /// <summary>
        /// Representa participaciones en el capital de una empresa.
        /// </summary>
        Accion = 1,

        /// <summary>
        /// Títulos de deuda pública o privada.
        /// </summary>
        Bono = 2,

        /// <summary>
        /// Fondos Comunes de Inversión.
        /// </summary>
        FCI = 3
    }
}
