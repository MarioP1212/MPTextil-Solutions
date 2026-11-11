using System.Net;
using MPTextil.Application.Solicitudes.Consultas;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Tests.Integracion.Soporte;

namespace MPTextil.Tests.Integracion;

[Collection(ColeccionApi.Nombre)]
public sealed class ConsultasApiTests(ApiDePrueba api)
{
    private static readonly string[] CodigosDeEjemplo = Enumerable.Range(1, 15).Select(numero => $"SOL-{numero:00000}").ToArray();

    [Fact]
    public async Task Listar_ComoCoordinador_IncluyeLasQuinceSolicitudesDeEjemplo()
    {
        var solicitudes = await ListarAsync(UsuariosDemo.Carlos, "api/solicitudes");

        Assert.All(CodigosDeEjemplo, codigo => Assert.Contains(solicitudes, solicitud => solicitud.Codigo == codigo));
    }

    [Fact]
    public async Task Listar_ComoProveedor_SoloDevuelveLasDeSuEmpresa()
    {
        var solicitudes = await ListarAsync(UsuariosDemo.Lucia, "api/solicitudes");

        Assert.NotEmpty(solicitudes);
        Assert.All(solicitudes, solicitud => Assert.Equal(UsuariosDemo.HilosDelValle, solicitud.Proveedor));
    }

    [Fact]
    public async Task ListarPorEstado_DevuelveSoloEseEstado()
    {
        var solicitudes = await ListarAsync(UsuariosDemo.Carlos, "api/solicitudes/Aprobado");

        Assert.NotEmpty(solicitudes);
        Assert.All(solicitudes, solicitud => Assert.Equal(EstadoSolicitud.Aprobado, solicitud.Estado));
    }

    [Fact]
    public async Task ListarPorEstado_ConEstadoInexistente_Devuelve400()
    {
        using var cliente = await api.ClienteComoAsync(UsuariosDemo.Carlos);

        using var respuesta = await cliente.GetAsync("api/solicitudes/Cancelado");

        var problema = await respuesta.LeerProblemaAsync();
        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.NotNull(problema.Errors);
        Assert.Contains("Estado", problema.Errors.Keys);
    }

    [Fact]
    public async Task ListarVencidas_DevuelveLasTresVencidasDeEjemplo()
    {
        var vencidas = await ListarAsync(UsuariosDemo.Carlos, "api/solicitudes/vencidas");

        Assert.Equal(["SOL-00006", "SOL-00007", "SOL-00015"], vencidas.Select(solicitud => solicitud.Codigo).Order());
        Assert.All(vencidas, solicitud => Assert.True(solicitud.EstaVencida));
    }

    [Fact]
    public async Task Detalle_DeUnaSolicitudDeOtroProveedor_Devuelve404()
    {
        var ajena = (await ListarAsync(UsuariosDemo.Carlos, "api/solicitudes"))
            .First(solicitud => solicitud.Proveedor == UsuariosDemo.HilosDelValle);
        using var pedro = await api.ClienteComoAsync(UsuariosDemo.Pedro);

        using var respuesta = await pedro.GetAsync($"api/solicitudes/{ajena.Id}");

        var problema = await respuesta.LeerProblemaAsync();
        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
        Assert.Equal("La solicitud no existe o no tienes acceso a ella.", problema.Detail);
    }

    [Fact]
    public async Task Detalle_SolicitudEvaluada_IncluyeFotosConEnlaceDeLectura()
    {
        var evaluada = (await ListarAsync(UsuariosDemo.Carlos, "api/solicitudes"))
            .Single(solicitud => solicitud.Codigo == "SOL-00010");
        using var carlos = await api.ClienteComoAsync(UsuariosDemo.Carlos);

        using var respuesta = await carlos.GetAsync($"api/solicitudes/{evaluada.Id}");

        var detalle = await respuesta.LeerAsync<SolicitudDetalle>();
        var evaluacion = detalle.Entregas[^1].Envio?.Evaluacion;
        Assert.NotNull(evaluacion);
        Assert.Equal(2, evaluacion.Fotos.Count);
        Assert.All(evaluacion.Fotos, foto => Assert.StartsWith("https://almacenamiento.prueba/datos-ejemplo/etiqueta/", foto.Url));
        Assert.All(evaluacion.Fotos, foto => Assert.Contains(NombreArchivo(foto.Url), api.Almacenamiento.Nombres));
    }

    [Fact]
    public async Task Detalle_SolicitudSinAsignar_OfreceAsignarSoloAlCoordinador()
    {
        var solicitada = (await ListarAsync(UsuariosDemo.Carlos, "api/solicitudes"))
            .Single(solicitud => solicitud.Codigo == "SOL-00001");
        using var carlos = await api.ClienteComoAsync(UsuariosDemo.Carlos);
        using var sofia = await api.ClienteComoAsync(UsuariosDemo.Sofia);

        var paraCarlos = await (await carlos.GetAsync($"api/solicitudes/{solicitada.Id}")).LeerAsync<SolicitudDetalle>();
        var paraSofia = await (await sofia.GetAsync($"api/solicitudes/{solicitada.Id}")).LeerAsync<SolicitudDetalle>();

        Assert.Equal([AccionSolicitud.Asignar], paraCarlos.AccionesDisponibles);
        Assert.Empty(paraSofia.AccionesDisponibles);
    }

    private static string NombreArchivo(string url) => new Uri(url).AbsolutePath.TrimStart('/');

    private async Task<List<SolicitudResumen>> ListarAsync(string correo, string ruta)
    {
        using var cliente = await api.ClienteComoAsync(correo);
        using var respuesta = await cliente.GetAsync(ruta);
        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);

        return await respuesta.LeerAsync<List<SolicitudResumen>>();
    }
}
