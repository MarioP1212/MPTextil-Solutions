namespace MPTextil.Infrastructure.Persistencia;

public sealed class OpcionesDatosEjemplo
{
    public const string Seccion = "DatosEjemplo";

    public bool Sembrar { get; init; }
    public string Contrasena { get; init; } = string.Empty;
}
