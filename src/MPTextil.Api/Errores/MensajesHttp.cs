namespace MPTextil.Api.Errores;

internal sealed record MensajeHttp(string Titulo, string Detalle);

internal static class MensajesHttp
{
    private static readonly Dictionary<int, MensajeHttp> Mensajes = new()
    {
        [StatusCodes.Status401Unauthorized] = new(
            "Sesión no válida",
            "Tu sesión expiró o no es válida. Inicia sesión de nuevo."),
        [StatusCodes.Status403Forbidden] = new(
            "Acceso denegado",
            "Tu usuario no tiene permiso para realizar esta acción."),
        [StatusCodes.Status404NotFound] = new(
            "No encontrado",
            "La dirección solicitada no existe."),
        [StatusCodes.Status405MethodNotAllowed] = new(
            "Operación no disponible",
            "Esta operación no está disponible en la dirección solicitada."),
        [StatusCodes.Status415UnsupportedMediaType] = new(
            "Formato no compatible",
            "El formato de los datos enviados no es compatible. Envía la información en formato JSON."),
        [StatusCodes.Status500InternalServerError] = new(
            "Error inesperado",
            "Ocurrió un problema inesperado. Intenta de nuevo y, si continúa, contacta al administrador.")
    };

    public static MensajeHttp? Buscar(int? codigoEstado) =>
        codigoEstado is { } codigo && Mensajes.TryGetValue(codigo, out var mensaje) ? mensaje : null;
}
