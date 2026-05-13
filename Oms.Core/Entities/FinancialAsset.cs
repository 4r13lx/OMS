using Oms.Core.Enums;

namespace Oms.Core.Entities
{
    /// <summary>
    /// Representa un activo financiero dentro del sistema (Acciones, Bonos, etc.).
    /// </summary>
    /// <remarks>
    /// Esta es una entidad de dominio que encapsula los datos fundamentales de un instrumento financiero.
    /// Se utiliza <c>sealed</c> para evitar la herencia si no es necesaria, promoviendo la composición.
    /// </remarks>
    public sealed class FinancialAsset
    {
        /// <summary>
        /// Identificador único del activo.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Símbolo de cotización (e.g., AAPL, GGAL).
        /// </summary>
        public string Ticker { get; set; } = null!;

        /// <summary>
        /// Nombre descriptivo del activo.
        /// </summary>
        public string Name { get; set; } = null!;

        /// <summary>
        /// Clasificación del activo según su naturaleza.
        /// </summary>
        public AssetType AssetType { get; set; }

        /// <summary>
        /// Precio base o de referencia del activo.
        /// </summary>
        public decimal BasePrice { get; set; }
    }
}
