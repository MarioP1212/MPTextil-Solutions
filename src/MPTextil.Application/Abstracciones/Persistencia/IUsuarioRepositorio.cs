using MPTextil.Domain.Entidades;

namespace MPTextil.Application.Abstracciones.Persistencia;

public interface IUsuarioRepositorio : IRepositorio<Usuario>
{
    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default);
}
