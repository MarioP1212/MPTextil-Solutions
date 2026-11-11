using MPTextil.Web.Modelos;

namespace MPTextil.Web.Servicios;

public sealed class ApiCatalogos(ClienteApi cliente)
{
    public Task<ResultadoApi<List<ElementoCatalogo>>> ListarProveedoresAsync() =>
        cliente.ObtenerAsync<List<ElementoCatalogo>>("api/catalogos/proveedores");

    public Task<ResultadoApi<List<ElementoCatalogo>>> ListarTiposMaterialAsync() =>
        cliente.ObtenerAsync<List<ElementoCatalogo>>("api/catalogos/tipos-material");

    public Task<ResultadoApi<List<OpcionCatalogo>>> ListarPrioridadesAsync() =>
        cliente.ObtenerAsync<List<OpcionCatalogo>>("api/catalogos/prioridades");

    public Task<ResultadoApi<List<OpcionCatalogo>>> ListarEstadosAsync() =>
        cliente.ObtenerAsync<List<OpcionCatalogo>>("api/catalogos/estados");
}
