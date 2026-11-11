using MPTextil.Domain.Comun;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Flujo;

namespace MPTextil.Domain.Entidades;

public sealed class Usuario : Entidad
{
    public const int NombreMaximo = 100;
    public const int CorreoMaximo = 150;
    public const int PasswordHashMaximo = 500;

    private Usuario()
    {
    }

    public string Nombre { get; private set; } = string.Empty;
    public string Correo { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public Rol Rol { get; private set; }
    public int? ProveedorId { get; private set; }
    public Proveedor? Proveedor { get; private set; }
    public bool Activo { get; private set; }
    public DateTime FechaCreacion { get; private set; }

    public static Usuario Crear(string nombre, string correo, string passwordHash, Rol rol, int? proveedorId, DateTime fechaCreacionUtc)
    {
        Guardia.Definido(rol, "rol");
        Guardia.Validar(rol != Rol.Proveedor || proveedorId is not null, "Un usuario con rol Proveedor debe pertenecer a un proveedor.");
        Guardia.Validar(rol == Rol.Proveedor || proveedorId is null, "Solo los usuarios con rol Proveedor pueden pertenecer a un proveedor.");

        return new Usuario
        {
            Nombre = Guardia.TextoObligatorio(nombre, "nombre", NombreMaximo),
            Correo = Guardia.Correo(correo, "correo", CorreoMaximo),
            PasswordHash = Guardia.TextoObligatorio(passwordHash, "contraseña", PasswordHashMaximo),
            Rol = rol,
            ProveedorId = proveedorId,
            Activo = true,
            FechaCreacion = fechaCreacionUtc
        };
    }

    public Actor ComoActor() => new(Id, Rol, ProveedorId);
}
