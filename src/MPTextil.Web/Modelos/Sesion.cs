namespace MPTextil.Web.Modelos;

public sealed record SolicitudInicioSesion(string Correo, string Contrasena);

public sealed record RespuestaInicioSesion(string Token, DateTime ExpiraUtc, PerfilUsuario Usuario);

public sealed record PerfilUsuario(int Id, string Nombre, string Correo, Rol Rol, int? ProveedorId, string? Proveedor);

public sealed record DatosSesion(string Token, DateTime ExpiraUtc, PerfilUsuario Usuario)
{
    public bool EstaVigente => ExpiraUtc > DateTime.UtcNow;
}
