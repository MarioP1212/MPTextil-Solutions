using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using MPTextil.Api.Formularios;
using MPTextil.Api.Validacion;

namespace MPTextil.Api.Configuracion;

internal static class ConfiguracionControladores
{
    private const string CampoGeneral = "Solicitud";
    private const string MensajeFormatoInvalido = "Este dato no tiene el formato esperado.";

    public static IServiceCollection AgregarControladores(this IServiceCollection servicios)
    {
        servicios
            .AddControllers(opciones =>
            {
                opciones.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
                opciones.Filters.Add<FiltroValidacion>();
            })
            .AddJsonOptions(opciones => opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
            .ConfigureApiBehaviorOptions(opciones => opciones.InvalidModelStateResponseFactory = CrearRespuestaModeloInvalido);

        servicios.AddValidatorsFromAssemblyContaining<ValidadorFormularioEvaluacion>();

        return servicios;
    }

    private static IActionResult CrearRespuestaModeloInvalido(ActionContext contexto)
    {
        var fabrica = contexto.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
        var problema = fabrica.CreateValidationProblemDetails(
            contexto.HttpContext,
            CrearEstadoAmigable(contexto.ModelState),
            StatusCodes.Status400BadRequest,
            "Revisa los datos enviados",
            detail: "La información enviada está incompleta o tiene un formato que no se puede leer.");

        return new BadRequestObjectResult(problema);
    }

    private static ModelStateDictionary CrearEstadoAmigable(ModelStateDictionary estadoOriginal)
    {
        var estadoAmigable = new ModelStateDictionary();

        foreach (var (clave, entrada) in estadoOriginal)
        {
            if (entrada.Errors.Count > 0)
            {
                estadoAmigable.AddModelError(NombreCampo(clave), MensajeFormatoInvalido);
            }
        }

        return estadoAmigable;
    }

    private static string NombreCampo(string clave)
    {
        var nombre = clave.TrimStart('$', '.');

        return string.IsNullOrEmpty(nombre) ? CampoGeneral : char.ToUpperInvariant(nombre[0]) + nombre[1..];
    }
}
