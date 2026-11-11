using Microsoft.EntityFrameworkCore;
using MPTextil.Application.Abstracciones.Persistencia;
using MPTextil.Domain.Comun;

namespace MPTextil.Infrastructure.Persistencia.Repositorios;

internal abstract class RepositorioBase<TEntidad>(MPTextilDbContext contexto) : IRepositorio<TEntidad>
    where TEntidad : Entidad
{
    protected DbSet<TEntidad> Conjunto { get; } = contexto.Set<TEntidad>();

    public virtual Task<TEntidad?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        Conjunto.FirstOrDefaultAsync(entidad => entidad.Id == id, cancellationToken);

    public void Agregar(TEntidad entidad) => Conjunto.Add(entidad);
}
