using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Persistencia.DatosEjemplo;

internal static class FotosEjemplo
{
    public const string Prefijo = "datos-ejemplo/";
    public const string ContentType = "image/png";

    private const string PrefijoRecurso = "FotosEjemplo.";
    private const string Extension = ".png";

    private static readonly HashSet<string> Muestras = ["tela", "malla", "etiqueta", "hilo"];

    public static string MuestraPara(TipoMaterial tipoMaterial)
    {
        var muestra = tipoMaterial.Nombre.ToLowerInvariant();

        return Muestras.Contains(muestra)
            ? muestra
            : throw new InvalidOperationException($"No hay foto de ejemplo para el tipo de material '{tipoMaterial.Nombre}'.");
    }

    public static string NombreArchivo(string muestra) => $"{Prefijo}{muestra}/{Guid.NewGuid():N}{Extension}";

    public static string? MuestraDe(string nombreArchivo)
    {
        if (!nombreArchivo.StartsWith(Prefijo, StringComparison.Ordinal))
        {
            return null;
        }

        var resto = nombreArchivo[Prefijo.Length..];
        var separador = resto.IndexOf('/');

        return separador > 0 && Muestras.Contains(resto[..separador]) ? resto[..separador] : null;
    }

    public static Stream Abrir(string muestra) =>
        typeof(FotosEjemplo).Assembly.GetManifestResourceStream($"{PrefijoRecurso}{muestra}{Extension}")
        ?? throw new InvalidOperationException($"No se encontró la foto de ejemplo '{muestra}'.");

    public static long Tamano(string muestra)
    {
        using var contenido = Abrir(muestra);
        return contenido.Length;
    }
}
