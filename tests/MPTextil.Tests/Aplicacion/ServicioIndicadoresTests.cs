using MPTextil.Application.Abstracciones.Consultas;
using MPTextil.Application.Indicadores;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Flujo;
using MPTextil.Tests.Aplicacion.Soporte;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Aplicacion;

public sealed class ServicioIndicadoresTests
{
    private const int DiaDeConsulta = 10;
    private const int ProveedorPuntualId = 30;

    [Fact]
    public async Task Obtener_ConEntregaATiempoTardeYVencida_CalculaElPorcentaje()
    {
        var consultas = new ConsultasIndicadoresFalsas(CrearDatos());

        var resumen = await CrearServicio(Actores.Coordinador, consultas).ObtenerAsync(CancellationToken.None);

        var textiles = resumen.CumplimientoPorProveedor.Single(proveedor => proveedor.ProveedorId == DatosPrueba.ProveedorAsignadoId);
        Assert.Equal(1, textiles.EntregasATiempo);
        Assert.Equal(1, textiles.EntregasTarde);
        Assert.Equal(1, textiles.VencidosSinEnvio);
        Assert.Equal(3, textiles.TotalMedido);
        Assert.Equal(33.3m, textiles.PorcentajeATiempo);
    }

    [Fact]
    public async Task Obtener_OrdenaPorPorcentajeYDejaAlFinalAlProveedorSinEntregas()
    {
        var consultas = new ConsultasIndicadoresFalsas(CrearDatos());

        var resumen = await CrearServicio(Actores.Coordinador, consultas).ObtenerAsync(CancellationToken.None);

        Assert.Equal(
            [ProveedorPuntualId, DatosPrueba.ProveedorAsignadoId, DatosPrueba.OtroProveedorId],
            resumen.CumplimientoPorProveedor.Select(proveedor => proveedor.ProveedorId));
        var ultimo = resumen.CumplimientoPorProveedor[^1];
        Assert.Null(ultimo.PorcentajeATiempo);
        Assert.Equal(0, ultimo.TotalMedido);
    }

    [Fact]
    public async Task Obtener_CalculaElPorcentajeGeneralConTodasLasEntregasMedidas()
    {
        var consultas = new ConsultasIndicadoresFalsas(CrearDatos());

        var resumen = await CrearServicio(Actores.Coordinador, consultas).ObtenerAsync(CancellationToken.None);

        Assert.Equal(60m, resumen.PorcentajeATiempoGeneral);
    }

    [Fact]
    public async Task Obtener_SinEntregasMedidas_NoTienePorcentajeGeneral()
    {
        var datos = CrearDatos() with { EntregasPorProveedor = [new EntregasProveedor(DatosPrueba.OtroProveedorId, "Hilos del Valle", 0, 0, 0)] };
        var consultas = new ConsultasIndicadoresFalsas(datos);

        var resumen = await CrearServicio(Actores.Coordinador, consultas).ObtenerAsync(CancellationToken.None);

        Assert.Null(resumen.PorcentajeATiempoGeneral);
    }

    [Fact]
    public async Task Obtener_IncluyeTodosLosEstadosAunqueNoTenganSolicitudes()
    {
        var consultas = new ConsultasIndicadoresFalsas(CrearDatos());

        var resumen = await CrearServicio(Actores.Coordinador, consultas).ObtenerAsync(CancellationToken.None);

        Assert.Equal(Enum.GetValues<EstadoSolicitud>(), resumen.SolicitudesPorEstado.Select(conteo => conteo.Estado));
        Assert.Equal(2, resumen.SolicitudesPorEstado.Single(conteo => conteo.Estado == EstadoSolicitud.EnProgreso).Cantidad);
        Assert.Equal(0, resumen.SolicitudesPorEstado.Single(conteo => conteo.Estado == EstadoSolicitud.Aprobado).Cantidad);
        Assert.Equal(3, resumen.TotalSolicitudes);
        Assert.Equal(1, resumen.SolicitudesVencidas);
        Assert.Equal(DatosPrueba.Hoy.AddDays(DiaDeConsulta), resumen.Fecha);
    }

    [Fact]
    public async Task Obtener_ComoProveedor_ConsultaSoloSuEmpresa()
    {
        var consultas = new ConsultasIndicadoresFalsas(CrearDatos());

        await CrearServicio(Actores.Proveedor, consultas).ObtenerAsync(CancellationToken.None);

        Assert.Equal(DatosPrueba.ProveedorAsignadoId, consultas.ProveedorConsultado);
    }

    [Fact]
    public async Task Obtener_ComoCoordinador_ConsultaTodosLosProveedores()
    {
        var consultas = new ConsultasIndicadoresFalsas(CrearDatos());

        await CrearServicio(Actores.Coordinador, consultas).ObtenerAsync(CancellationToken.None);

        Assert.Null(consultas.ProveedorConsultado);
    }

    [Fact]
    public async Task Obtener_ConsultaConLaFechaDelReloj()
    {
        var consultas = new ConsultasIndicadoresFalsas(CrearDatos());

        await CrearServicio(Actores.Coordinador, consultas).ObtenerAsync(CancellationToken.None);

        Assert.Equal(DatosPrueba.Hoy.AddDays(DiaDeConsulta), consultas.FechaConsultada);
    }

    private static ServicioIndicadores CrearServicio(Actor actor, ConsultasIndicadoresFalsas consultas) =>
        new(consultas, new UsuarioActualFalso(actor), new RelojFijo(DiaDeConsulta));

    private static DatosIndicadores CrearDatos() => new(
        new Dictionary<EstadoSolicitud, int> { [EstadoSolicitud.EnProgreso] = 2, [EstadoSolicitud.Enviado] = 1 },
        1,
        [
            new EntregasProveedor(DatosPrueba.OtroProveedorId, "Hilos del Valle", 0, 0, 0),
            new EntregasProveedor(DatosPrueba.ProveedorAsignadoId, "Textiles del Norte", 1, 1, 1),
            new EntregasProveedor(ProveedorPuntualId, "Mallas del Caribe", 2, 0, 0)
        ]);

    private sealed class ConsultasIndicadoresFalsas(DatosIndicadores datos) : IConsultasIndicadores
    {
        public int? ProveedorConsultado { get; private set; }
        public DateOnly? FechaConsultada { get; private set; }

        public Task<DatosIndicadores> ObtenerAsync(DateOnly hoy, int? proveedorId, CancellationToken cancellationToken)
        {
            FechaConsultada = hoy;
            ProveedorConsultado = proveedorId;
            return Task.FromResult(datos);
        }
    }
}
