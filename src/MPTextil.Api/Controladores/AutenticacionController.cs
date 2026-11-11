using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Application.Autenticacion;

namespace MPTextil.Api.Controladores;

[ApiController]
[Route("api/autenticacion")]
[Tags("Autenticación")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class AutenticacionController(
    ServicioAutenticacion servicioAutenticacion,
    IUsuarioActual usuarioActual) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("iniciar-sesion")]
    [EndpointSummary("Iniciar sesión")]
    [EndpointDescription("Valida el correo y la contraseña y devuelve el token JWT junto con el perfil del usuario.")]
    [ProducesResponseType<RespuestaInicioSesion>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RespuestaInicioSesion>> IniciarSesion(
        SolicitudInicioSesion solicitud,
        CancellationToken cancellationToken) =>
        await servicioAutenticacion.IniciarSesionAsync(solicitud, cancellationToken);

    [HttpGet("perfil")]
    [EndpointSummary("Consultar mi perfil")]
    [EndpointDescription("Devuelve los datos del usuario dueño del token.")]
    [ProducesResponseType<PerfilUsuario>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PerfilUsuario>> ObtenerPerfil(CancellationToken cancellationToken) =>
        await servicioAutenticacion.ObtenerPerfilAsync(usuarioActual.Actor.UsuarioId, cancellationToken);
}
