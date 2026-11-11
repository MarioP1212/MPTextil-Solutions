using Microsoft.EntityFrameworkCore;
using MPTextil.Application.Abstracciones.Persistencia;
using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Persistencia.Repositorios;

internal sealed class TipoMaterialRepositorio(MPTextilDbContext contexto)
    : RepositorioBase<TipoMaterial>(contexto), ITipoMaterialRepositorio
{
    public async Task<IReadOnlyList<TipoMaterial>> ListarActivosAsync(CancellationToken cancellationToken = default) =>
        await Conjunto
            .AsNoTracking()
            .Where(tipo => tipo.Activo)
            .OrderBy(tipo => tipo.Nombre)
            .ToListAsync(cancellationToken);
}
