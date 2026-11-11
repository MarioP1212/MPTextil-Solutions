using MPTextil.Web.Modelos;

namespace MPTextil.Web.Servicios;

public sealed class ApiIndicadores(ClienteApi cliente)
{
    public Task<ResultadoApi<ResumenIndicadores>> ObtenerAsync() =>
        cliente.ObtenerAsync<ResumenIndicadores>("api/indicadores");
}
