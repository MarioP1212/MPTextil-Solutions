using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Application.Solicitudes.Acciones;

public sealed record RegistrarEvaluacion(
    ResultadoEvaluacion Resultado,
    string Comentarios,
    IReadOnlyList<ArchivoFoto> Fotos);

public sealed record ArchivoFoto(
    string NombreOriginal,
    string ContentType,
    long TamanoBytes,
    Func<Stream> AbrirLectura);
