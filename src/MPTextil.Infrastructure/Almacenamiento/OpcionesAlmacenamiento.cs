namespace MPTextil.Infrastructure.Almacenamiento;

public sealed class OpcionesAlmacenamiento
{
    public const string Seccion = "Almacenamiento";

    public string CadenaConexion { get; init; } = string.Empty;
    public string Contenedor { get; init; } = "fotos-muestras";
    public int MinutosVigenciaEnlaces { get; init; } = 60;

    public bool EsValida =>
        !string.IsNullOrWhiteSpace(CadenaConexion)
        && !string.IsNullOrWhiteSpace(Contenedor)
        && MinutosVigenciaEnlaces > 0;
}
