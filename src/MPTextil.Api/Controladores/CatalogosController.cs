using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using MPTextil.Application.Catalogos;

namespace MPTextil.Api.Controladores;

[ApiController]
[Route("api/catalogos")]
[Tags("Catálogos")]
[Produces(MediaTypeNames.Application.Json)]
[ProducesResponseType<ProblemDetails>(StatusCodes.Status401Unauthorized)]
public sealed class CatalogosController(ServicioCatalogos servicioCatalogos) : ControllerBase
{
    [HttpGet("proveedores")]
    [EndpointSummary("Listar proveedores activos")]
    [ProducesResponseType<IReadOnlyList<ElementoCatalogo>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ElementoCatalogo>>> ListarProveedores(CancellationToken cancellationToken) =>
        Ok(await servicioCatalogos.ListarProveedoresAsync(cancellationToken));

    [HttpGet("tipos-material")]
    [EndpointSummary("Listar tipos de material")]
    [ProducesResponseType<IReadOnlyList<ElementoCatalogo>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ElementoCatalogo>>> ListarTiposMaterial(CancellationToken cancellationToken) =>
        Ok(await servicioCatalogos.ListarTiposMaterialAsync(cancellationToken));

    [HttpGet("prioridades")]
    [EndpointSummary("Listar prioridades")]
    [ProducesResponseType<IReadOnlyList<OpcionCatalogo>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<OpcionCatalogo>> ListarPrioridades() =>
        Ok(servicioCatalogos.ListarPrioridades());

    [HttpGet("estados")]
    [EndpointSummary("Listar estados de solicitud")]
    [ProducesResponseType<IReadOnlyList<OpcionCatalogo>>(StatusCodes.Status200OK)]
    public ActionResult<IReadOnlyList<OpcionCatalogo>> ListarEstados() =>
        Ok(servicioCatalogos.ListarEstados());
}
