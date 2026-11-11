using System.Net;
using MPTextil.Application.Indicadores;
using MPTextil.Tests.Integracion.Soporte;

namespace MPTextil.Tests.Integracion;

[Collection(ColeccionApi.Nombre)]
public sealed class IndicadoresApiTests(ApiDePrueba api)
{
    [Fact]
    public async Task Indicadores_ComoCoordinador_CalculaElCumplimientoDeCadaProveedor()
    {
        var resumen = await ObtenerAsync(UsuariosDemo.Carlos);

        Assert.Equal(4, resumen.CumplimientoPorProveedor.Count);
        Assert.Equal(100m, PorcentajeDe(resumen, UsuariosDemo.TextilesDelNorte));
        Assert.Equal(50m, PorcentajeDe(resumen, "Etiquetas Centroamericanas"));
        Assert.Equal(50m, PorcentajeDe(resumen, "Mallas Técnicas del Caribe"));
        Assert.Equal(33.3m, PorcentajeDe(resumen, UsuariosDemo.HilosDelValle));
        Assert.Equal(3, resumen.SolicitudesVencidas);
        Assert.Equal(resumen.TotalSolicitudes, resumen.SolicitudesPorEstado.Sum(conteo => conteo.Cantidad));
        Assert.Equal(58.3m, resumen.PorcentajeATiempoGeneral);
    }

    [Fact]
    public async Task Indicadores_ComoProveedor_SoloMuestraSuEmpresa()
    {
        var resumen = await ObtenerAsync(UsuariosDemo.Lucia);

        var cumplimiento = Assert.Single(resumen.CumplimientoPorProveedor);
        Assert.Equal(UsuariosDemo.HilosDelValle, cumplimiento.Proveedor);
        Assert.Equal(1, cumplimiento.EntregasATiempo);
        Assert.Equal(1, cumplimiento.EntregasTarde);
        Assert.Equal(1, cumplimiento.VencidosSinEnvio);
    }

    [Fact]
    public async Task Indicadores_SinToken_Devuelve401()
    {
        using var cliente = api.ClienteAnonimo();

        using var respuesta = await cliente.GetAsync("api/indicadores");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    private static decimal? PorcentajeDe(ResumenIndicadores resumen, string proveedor) =>
        resumen.CumplimientoPorProveedor.Single(cumplimiento => cumplimiento.Proveedor == proveedor).PorcentajeATiempo;

    private async Task<ResumenIndicadores> ObtenerAsync(string correo)
    {
        using var cliente = await api.ClienteComoAsync(correo);
        using var respuesta = await cliente.GetAsync("api/indicadores");
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        return await respuesta.LeerAsync<ResumenIndicadores>();
    }
}
