using Microsoft.EntityFrameworkCore;
using MPTextil.Application.Abstracciones.Persistencia;
using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Persistencia.Repositorios;

internal sealed class ProveedorRepositorio(MPTextilDbContext contexto)
    : RepositorioBase<Proveedor>(contexto), IProveedorRepositorio
{
    public async Task<IReadOnlyList<Proveedor>> ListarActivosAsync(CancellationToken cancellationToken = default) =>
        await Conjunto
            .AsNoTracking()
            .Where(proveedor => proveedor.Activo)
            .OrderBy(proveedor => proveedor.Nombre)
            .ToListAsync(cancellationToken);
}
