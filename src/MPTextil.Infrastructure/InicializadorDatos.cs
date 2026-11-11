using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MPTextil.Application.Abstracciones.Almacenamiento;
using MPTextil.Infrastructure.Persistencia;
using MPTextil.Infrastructure.Persistencia.DatosEjemplo;

namespace MPTextil.Infrastructure;

public static class InicializadorDatos
{
    public static async Task InicializarDatosAsync(
        this IServiceProvider servicios,
        OpcionesDatosEjemplo datosEjemplo,
        CancellationToken cancellationToken = default)
    {
        await using var alcance = servicios.CreateAsyncScope();
        var proveedor = alcance.ServiceProvider;

        await proveedor.GetRequiredService<MPTextilDbContext>().Database.MigrateAsync(cancellationToken);
        await proveedor.GetRequiredService<IAlmacenamientoArchivos>().PrepararAsync(cancellationToken);

        if (!datosEjemplo.Sembrar)
        {
            return;
        }

        await proveedor.GetRequiredService<SembradorDatosEjemplo>().SembrarAsync(datosEjemplo.Contrasena, cancellationToken);
        await proveedor.GetRequiredService<SincronizadorFotosEjemplo>().SincronizarAsync(cancellationToken);
    }
}
