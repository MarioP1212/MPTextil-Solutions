using MPTextil.Domain.Comun;

namespace MPTextil.Domain.Entidades;

public sealed class TipoMaterial : Entidad
{
    public const int NombreMaximo = 50;

    private TipoMaterial()
    {
    }

    public string Nombre { get; private set; } = string.Empty;
    public bool Activo { get; private set; }

    public static TipoMaterial Crear(string nombre) => new()
    {
        Nombre = Guardia.TextoObligatorio(nombre, "nombre del tipo de material", NombreMaximo),
        Activo = true
    };

    public void Desactivar() => Activo = false;
}
