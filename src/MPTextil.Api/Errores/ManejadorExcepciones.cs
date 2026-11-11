using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MPTextil.Application.Excepciones;
using MPTextil.Domain.Excepciones;

namespace MPTextil.Api.Errores;

internal sealed class ManejadorExcepciones(
    IProblemDetailsService servicioProblemas,
    ILogger<ManejadorExcepciones> registro) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext contexto, Exception excepcion, CancellationToken cancellationToken)
    {
        var problema = CrearProblema(excepcion);

        if (problema.Status == StatusCodes.Status500InternalServerError)
        {
            registro.LogError(excepcion, "Error no controlado en {Metodo} {Ruta}", contexto.Request.Method, contexto.Request.Path);
        }

        contexto.Response.StatusCode = problema.Status ?? StatusCodes.Status500InternalServerError;

        return await servicioProblemas.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexto,
            ProblemDetails = problema,
            Exception = excepcion
        });
    }

    private static ProblemDetails CrearProblema(Exception excepcion) => excepcion switch
    {
        ValidacionException validacion => new ValidationProblemDetails(validacion.Errores)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Revisa los datos ingresados",
            Detail = validacion.Message
        },
        ReglaNegocioException => Problema(StatusCodes.Status400BadRequest, "No se pudo completar la acción", excepcion.Message),
        CredencialesInvalidasException => Problema(StatusCodes.Status401Unauthorized, "No se pudo iniciar sesión", excepcion.Message),
        AccesoDenegadoException => Problema(StatusCodes.Status403Forbidden, "Acción no permitida", excepcion.Message),
        RecursoNoEncontradoException => Problema(StatusCodes.Status404NotFound, "No encontrado", excepcion.Message),
        TransicionInvalidaException => Problema(StatusCodes.Status409Conflict, "Acción no disponible", excepcion.Message),
        ConflictoConcurrenciaException => Problema(StatusCodes.Status409Conflict, "La información cambió", excepcion.Message),
        RegistroDuplicadoException => Problema(StatusCodes.Status409Conflict, "Registro duplicado", excepcion.Message),
        _ => ProblemaInesperado()
    };

    private static ProblemDetails Problema(int estado, string titulo, string detalle) => new()
    {
        Status = estado,
        Title = titulo,
        Detail = detalle
    };

    private static ProblemDetails ProblemaInesperado()
    {
        var mensaje = MensajesHttp.Buscar(StatusCodes.Status500InternalServerError);

        return Problema(StatusCodes.Status500InternalServerError, mensaje?.Titulo ?? string.Empty, mensaje?.Detalle ?? string.Empty);
    }
}
