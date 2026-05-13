using Microsoft.Extensions.DependencyInjection;

namespace Oms.Data
{
    /// <summary>
    /// Extensiones para el ServiceProvider que facilitan tareas de configuración al inicio de la aplicación.
    /// Este enfoque centraliza la lógica de inicialización, como la creación de la base de datos.
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// Asegura que la base de datos exista al iniciar la aplicación.
        /// Es una tarea común de infraestructura que se ejecuta durante el arranque (startup)
        /// para garantizar que el entorno esté listo para operar.
        /// </summary>
        public static void EnsureDatabaseCreated(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OmsDbContext>();
            dbContext.Database.EnsureCreated();
        }
    }
}
