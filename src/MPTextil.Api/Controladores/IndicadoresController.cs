using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using MPTextil.Application.Indicadores;

namespace MPTextil.Api.Controladores;

[ApiController]
[Route("api/indicadores")]
[Tags("Indicadores")]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
public sealed class IndicadoresController(ServicioIndicadores servicioIndicadores) : ControllerBase
{
    [HttpGet]
    [EndpointSummary("Consultar indicadores")]
    [EndpointDescription("Solicitudes por estado, solicitudes vencidas y porcentaje de entregas a tiempo por proveedor: entregas a tiempo entre el total de entregas a tiempo, entregas tarde y compromisos vencidos sin envío. El proveedor solo ve sus propios indicadores.")]
    [ProducesResponseType<ResumenIndicadores>(StatusCodes.Status200OK)]
    public async Task<ActionResult<ResumenIndicadores>> Obtener(CancellationToken cancellationToken) =>
        await servicioIndicadores.ObtenerAsync(cancellationToken);
}
