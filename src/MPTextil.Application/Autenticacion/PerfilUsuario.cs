using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Application.Autenticacion;

public sealed record PerfilUsuario(int Id, string Nombre, string Correo, Rol Rol, int? ProveedorId, string? Proveedor)
{
    public static PerfilUsuario Desde(Usuario usuario) =>
        new(usuario.Id, usuario.Nombre, usuario.Correo, usuario.Rol, usuario.ProveedorId, usuario.Proveedor?.Nombre);
}
