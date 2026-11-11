using MPTextil.Domain.Entidades;

namespace MPTextil.Application.Abstracciones.Persistencia;

public interface IProveedorRepositorio : IRepositorio<Proveedor>
{
    Task<IReadOnlyList<Proveedor>> ListarActivosAsync(CancellationToken cancellationToken = default);
}
