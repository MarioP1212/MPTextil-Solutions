using MPTextil.Domain.Entidades;

namespace MPTextil.Application.Abstracciones.Persistencia;

public interface ITipoMaterialRepositorio : IRepositorio<TipoMaterial>
{
    Task<IReadOnlyList<TipoMaterial>> ListarActivosAsync(CancellationToken cancellationToken = default);
}
