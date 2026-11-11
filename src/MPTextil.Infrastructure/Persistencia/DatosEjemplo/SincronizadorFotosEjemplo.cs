using Microsoft.EntityFrameworkCore;
using MPTextil.Application.Abstracciones.Almacenamiento;
using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Persistencia.DatosEjemplo;

internal sealed class SincronizadorFotosEjemplo(MPTextilDbContext contexto, IAlmacenamientoArchivos almacenamiento)
{
    public async Task SincronizarAsync(CancellationToken cancellationToken)
    {
        var nombres = await contexto.Set<FotoEvaluacion>()
            .AsNoTracking()
            .Where(foto => foto.BlobNombre.StartsWith(FotosEjemplo.Prefijo))
            .Select(foto => foto.BlobNombre)
            .ToListAsync(cancellationToken);

        foreach (var nombre in nombres)
        {
            if (FotosEjemplo.MuestraDe(nombre) is not { } muestra || await almacenamiento.ExisteAsync(nombre, cancellationToken))
            {
                continue;
            }

            await using var contenido = FotosEjemplo.Abrir(muestra);
            await almacenamiento.GuardarAsync(nombre, contenido, FotosEjemplo.ContentType, cancellationToken);
        }
    }
}
