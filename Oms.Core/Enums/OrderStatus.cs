namespace Oms.Core.Enums
{
    /// <summary>
    /// Representa los estados posibles de una orden de inversión.
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// La orden ha sido creada pero aún no se ha procesado.
        /// </summary>
        EnProceso = 0,

        /// <summary>
        /// La orden ha sido confirmada y ejecutada en el mercado.
        /// </summary>
        Ejecutada = 1,

        /// <summary>
        /// La orden ha sido anulada, ya sea por el usuario o por el sistema.
        /// </summary>
        Cancelada = 3
    }
}
