using Microsoft.EntityFrameworkCore;
using MPTextil.Application.Abstracciones.Persistencia;
using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Persistencia.Repositorios;

internal sealed class UsuarioRepositorio(MPTextilDbContext contexto)
    : RepositorioBase<Usuario>(contexto), IUsuarioRepositorio
{
    public override Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        Conjunto
            .Include(usuario => usuario.Proveedor)
            .FirstOrDefaultAsync(usuario => usuario.Id == id, cancellationToken);

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken cancellationToken = default)
    {
        var correoNormalizado = correo.Trim().ToLowerInvariant();

        return Conjunto
            .Include(usuario => usuario.Proveedor)
            .FirstOrDefaultAsync(usuario => usuario.Correo == correoNormalizado, cancellationToken);
    }
}
