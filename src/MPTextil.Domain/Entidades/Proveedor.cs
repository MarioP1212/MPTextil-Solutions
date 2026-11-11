using MPTextil.Domain.Comun;

namespace MPTextil.Domain.Entidades;

public sealed class Proveedor : Entidad
{
    public const int NombreMaximo = 150;
    public const int CorreoMaximo = 150;
    public const int TelefonoMaximo = 30;

    private Proveedor()
    {
    }

    public string Nombre { get; private set; } = string.Empty;
    public string Correo { get; private set; } = string.Empty;
    public string? Telefono { get; private set; }
    public bool Activo { get; private set; }

    public static Proveedor Crear(string nombre, string correo, string? telefono) => new()
    {
        Nombre = Guardia.TextoObligatorio(nombre, "nombre del proveedor", NombreMaximo),
        Correo = Guardia.Correo(correo, "correo del proveedor", CorreoMaximo),
        Telefono = Guardia.TextoOpcional(telefono, "teléfono", TelefonoMaximo),
        Activo = true
    };

    public void Desactivar() => Activo = false;
}
