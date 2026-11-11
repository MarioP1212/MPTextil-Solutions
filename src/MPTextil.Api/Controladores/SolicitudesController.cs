using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPTextil.Api.Formularios;
using MPTextil.Api.Seguridad;
using MPTextil.Application.Solicitudes.Acciones;
using MPTextil.Application.Solicitudes.Consultas;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Api.Controladores;

[ApiController]
[Route("api/solicitudes")]
[Tags("Solicitudes")]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
public sealed class SolicitudesController(
    ServicioSolicitudes servicioSolicitudes,
    ServicioConsultaSolicitudes servicioConsulta) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Listar solicitudes")]
    [EndpointDescription("Devuelve las solicitudes de la más reciente a la más antigua. El proveedor solo ve las solicitudes asignadas a su empresa.")]
    [ProducesResponseType<IReadOnlyList<SolicitudResumen>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SolicitudResumen>>> Listar(CancellationToken cancellationToken) =>
        Ok(await servicioConsulta.ListarAsync(null, cancellationToken));

    [HttpGet("{estado:alpha}")]
    [EndpointSummary("Listar solicitudes por estado")]
    [EndpointDescription("Filtra por estado: Solicitado, Asignado, EnProgreso, Enviado, EnEvaluacion, PendienteAutorizacion o Aprobado. El proveedor solo ve las solicitudes asignadas a su empresa.")]
    [ProducesResponseType<IReadOnlyList<SolicitudResumen>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<SolicitudResumen>>> ListarPorEstado(
        EstadoSolicitud estado,
        CancellationToken cancellationToken) =>
        Ok(await servicioConsulta.ListarAsync(estado, cancellationToken));

    [HttpGet("vencidas")]
    [EndpointSummary("Listar solicitudes vencidas")]
    [EndpointDescription("Devuelve las solicitudes en progreso cuya fecha compromiso ya pasó sin que se registrara el envío.")]
    [ProducesResponseType<IReadOnlyList<SolicitudResumen>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SolicitudResumen>>> ListarVencidas(CancellationToken cancellationToken) =>
        Ok(await servicioConsulta.ListarVencidasAsync(cancellationToken));

    [HttpGet("{id:int}")]
    [EndpointSummary("Ver detalle de una solicitud")]
    [EndpointDescription("Incluye las entregas con su envío, evaluación y decisiones, el historial completo y las acciones que el usuario puede realizar.")]
    [ProducesResponseType<SolicitudDetalle>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SolicitudDetalle>> ObtenerDetalle(int id, CancellationToken cancellationToken) =>
        await servicioConsulta.ObtenerDetalleAsync(id, cancellationToken);

    [HttpPost]
    [Authorize(Policy = Politicas.Solicitante)]
    [EndpointSummary("Crear solicitud")]
    [EndpointDescription("Registra una nueva solicitud de desarrollo de material. Solo para el rol Solicitante.")]
    [ProducesResponseType<RespuestaAccion>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<RespuestaAccion>> Crear(CrearSolicitud datos, CancellationToken cancellationToken)
    {
        var respuesta = await servicioSolicitudes.CrearAsync(datos, cancellationToken);

        return CreatedAtAction(nameof(ObtenerDetalle), new { id = respuesta.SolicitudId }, respuesta);
    }

    [HttpPost("{id:int}/asignar")]
    [Authorize(Policy = Politicas.Coordinador)]
    [EndpointSummary("Asignar proveedor")]
    [EndpointDescription("Asigna la solicitud a un proveedor activo. Solo para el rol Coordinador y con la solicitud en estado Solicitado.")]
    [ProducesResponseType<RespuestaAccion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespuestaAccion>> Asignar(int id, AsignarProveedor datos, CancellationToken cancellationToken) =>
        await servicioSolicitudes.AsignarAsync(id, datos, cancellationToken);

    [HttpPost("{id:int}/declinar")]
    [Authorize(Policy = Politicas.Proveedor)]
    [EndpointSummary("Declinar asignación")]
    [EndpointDescription("El proveedor asignado rechaza atender la solicitud indicando el motivo. La solicitud regresa a Solicitado.")]
    [ProducesResponseType<RespuestaAccion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespuestaAccion>> Declinar(int id, MotivoAccion datos, CancellationToken cancellationToken) =>
        await servicioSolicitudes.DeclinarAsync(id, datos, cancellationToken);

    [HttpPost("{id:int}/aceptar")]
    [Authorize(Policy = Politicas.Proveedor)]
    [EndpointSummary("Aceptar solicitud")]
    [EndpointDescription("El proveedor asignado acepta la solicitud con su fecha compromiso. La solicitud pasa a En progreso.")]
    [ProducesResponseType<RespuestaAccion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespuestaAccion>> Aceptar(int id, FechaCompromisoSolicitud datos, CancellationToken cancellationToken) =>
        await servicioSolicitudes.AceptarAsync(id, datos, cancellationToken);

    [HttpPost("{id:int}/cambiar-fecha-compromiso")]
    [Authorize(Policy = Politicas.Proveedor)]
    [EndpointSummary("Cambiar fecha compromiso")]
    [EndpointDescription("Registra una nueva fecha compromiso después de que la muestra fue rechazada. La fecha anterior queda en el histórico.")]
    [ProducesResponseType<RespuestaAccion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespuestaAccion>> CambiarFechaCompromiso(
        int id,
        FechaCompromisoSolicitud datos,
        CancellationToken cancellationToken) =>
        await servicioSolicitudes.CambiarFechaCompromisoAsync(id, datos, cancellationToken);

    [HttpPost("{id:int}/registrar-envio")]
    [Authorize(Policy = Politicas.Proveedor)]
    [EndpointSummary("Registrar envío de la muestra")]
    [EndpointDescription("El proveedor registra la fecha de envío, el transportista y el número de guía. La solicitud pasa a Enviado.")]
    [ProducesResponseType<RespuestaAccion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespuestaAccion>> RegistrarEnvio(
        int id,
        RegistrarEnvioSolicitud datos,
        CancellationToken cancellationToken) =>
        await servicioSolicitudes.RegistrarEnvioAsync(id, datos, cancellationToken);

    [HttpPost("{id:int}/registrar-evaluacion")]
    [Authorize(Policy = Politicas.Coordinador)]
    [Consumes("multipart/form-data")]
    [EndpointSummary("Registrar evaluación de la muestra")]
    [EndpointDescription("El coordinador registra el resultado (Cumple, NoCumple o CumpleConObservaciones), sus comentarios y de 1 a 5 fotos JPG, PNG o WEBP de hasta 5 MB. La solicitud pasa a En evaluación.")]
    [ProducesResponseType<RespuestaAccion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespuestaAccion>> RegistrarEvaluacion(
        int id,
        [FromForm] FormularioEvaluacion formulario,
        CancellationToken cancellationToken) =>
        await servicioSolicitudes.RegistrarEvaluacionAsync(id, formulario.ARegistrarEvaluacion(), cancellationToken);

    [HttpPost("{id:int}/aprobar")]
    [Authorize(Policy = Politicas.Solicitante)]
    [EndpointSummary("Aprobar muestra")]
    [EndpointDescription("El solicitante que creó la solicitud aprueba la muestra evaluada. Queda pendiente de autorización.")]
    [ProducesResponseType<RespuestaAccion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespuestaAccion>> Aprobar(int id, CancellationToken cancellationToken) =>
        await servicioSolicitudes.AprobarAsync(id, cancellationToken);

    [HttpPost("{id:int}/autorizar")]
    [Authorize(Policy = Politicas.Autorizador)]
    [EndpointSummary("Autorizar muestra")]
    [EndpointDescription("El autorizador da el visto bueno final. La solicitud queda Aprobada y cerrada.")]
    [ProducesResponseType<RespuestaAccion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespuestaAccion>> Autorizar(int id, CancellationToken cancellationToken) =>
        await servicioSolicitudes.AutorizarAsync(id, cancellationToken);

    [HttpPost("{id:int}/rechazar")]
    [Authorize(Policy = Politicas.DecisorMuestra)]
    [EndpointSummary("Rechazar muestra")]
    [EndpointDescription("En evaluación rechaza el solicitante que creó la solicitud; pendiente de autorización rechaza el autorizador. La solicitud regresa a En progreso.")]
    [ProducesResponseType<RespuestaAccion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status403Forbidden)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RespuestaAccion>> Rechazar(int id, MotivoAccion datos, CancellationToken cancellationToken) =>
        await servicioSolicitudes.RechazarAsync(id, datos, cancellationToken);
}
