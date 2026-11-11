using MPTextil.Domain.Comun;
using MPTextil.Domain.Excepciones;

namespace MPTextil.Domain.Entidades;

public sealed class FotoEvaluacion : Entidad
{
    public const int BlobNombreMaximo = 200;
    public const int NombreOriginalMaximo = 200;
    public const int ContentTypeMaximo = 100;
    public const long TamanoMaximoBytes = 5 * 1024 * 1024;

    private const string MensajeTipoNoPermitido = "Solo se permiten fotos en formato JPG, PNG o WEBP.";

    private static readonly Dictionary<string, string> ExtensionesPorTipo = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/jpeg"] = ".jpg",
        ["image/png"] = ".png",
        ["image/webp"] = ".webp"
    };

    private FotoEvaluacion()
    {
    }

    public int EvaluacionId { get; private set; }
    public string BlobNombre { get; private set; } = string.Empty;
    public string NombreOriginal { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long TamanoBytes { get; private set; }
    public DateTime FechaSubida { get; private set; }

    public static bool EsTipoPermitido(string? contentType) =>
        contentType is not null && ExtensionesPorTipo.ContainsKey(contentType);

    public static string ExtensionDe(string contentType) =>
        ExtensionesPorTipo.TryGetValue(contentType, out var extension)
            ? extension
            : throw new ReglaNegocioException(MensajeTipoNoPermitido);

    public static FotoEvaluacion Crear(string blobNombre, string nombreOriginal, string contentType, long tamanoBytes, DateTime fechaSubidaUtc)
    {
        Guardia.Validar(EsTipoPermitido(contentType), MensajeTipoNoPermitido);
        Guardia.Validar(tamanoBytes is > 0 and <= TamanoMaximoBytes, $"Cada foto debe pesar como máximo {TamanoMaximoBytes / (1024 * 1024)} MB.");

        return new FotoEvaluacion
        {
            BlobNombre = Guardia.TextoObligatorio(blobNombre, "nombre del archivo", BlobNombreMaximo),
            NombreOriginal = Guardia.TextoObligatorio(nombreOriginal, "nombre original de la foto", NombreOriginalMaximo),
            ContentType = contentType.ToLowerInvariant(),
            TamanoBytes = tamanoBytes,
            FechaSubida = fechaSubidaUtc
        };
    }
}
