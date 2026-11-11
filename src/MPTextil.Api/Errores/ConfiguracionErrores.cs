namespace MPTextil.Api.Errores;

internal static class ConfiguracionErrores
{
    public static IServiceCollection AgregarManejoErrores(this IServiceCollection servicios)
    {
        servicios.AddProblemDetails(opciones => opciones.CustomizeProblemDetails = Personalizar);
        servicios.AddExceptionHandler<ManejadorExcepciones>();

        return servicios;
    }

    private static void Personalizar(ProblemDetailsContext contexto)
    {
        var problema = contexto.ProblemDetails;
        problema.Instance ??= contexto.HttpContext.Request.Path;

        if (contexto.Exception is null && MensajesHttp.Buscar(problema.Status) is { } mensaje)
        {
            problema.Title = mensaje.Titulo;
            problema.Detail ??= mensaje.Detalle;
        }
    }
}
