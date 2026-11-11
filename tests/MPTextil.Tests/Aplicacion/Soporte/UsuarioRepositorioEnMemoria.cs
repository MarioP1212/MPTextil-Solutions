using MPTextil.Application.Abstracciones.Persistencia;
using MPTextil.Domain.Entidades;

namespace MPTextil.Tests.Aplicacion.Soporte;

internal sealed class UsuarioRepositorioEnMemoria(params Usuario[] usuarios) : IUsuarioRepositorio
{
    private readonly List<Usuario> _usuarios = [.. usuarios];

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_usuarios.FirstOrDefault(usuario => usuario.Id == id));

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default) =>
        Task.FromResult(_usuarios.FirstOrDefault(usuario => usuario.Correo == correo.Trim().ToLowerInvariant()));

    public void Agregar(Usuario entidad) => _usuarios.Add(entidad);
}
