using MPTextil.Domain.Comun;

namespace MPTextil.Application.Abstracciones.Persistencia;

public interface IRepositorio<TEntidad> where TEntidad : Entidad
{
    Task<TEntidad?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default);

    void Agregar(TEntidad entidad);
}
