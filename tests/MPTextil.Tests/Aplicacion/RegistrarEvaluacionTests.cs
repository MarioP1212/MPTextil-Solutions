using MPTextil.Application.Excepciones;
using MPTextil.Application.Solicitudes.Acciones;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Excepciones;
using MPTextil.Domain.Flujo;
using MPTextil.Tests.Aplicacion.Soporte;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Aplicacion;

public sealed class RegistrarEvaluacionTests
{
    private const int SolicitudId = 42;

    private static readonly byte[] ContenidoPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D];

    private readonly AlmacenamientoEnMemoria _almacenamiento = new();
    private readonly Solicitud _solicitud = Escenarios.Enviada().ConId(SolicitudId);

    [Fact]
    public async Task RegistrarEvaluacion_ConFotosValidas_SubeLosArchivosYPasaAEnEvaluacion()
    {
        var unidadDeTrabajo = new UnidadDeTrabajoFalsa();
        var servicio = CrearServicio(Actores.Coordinador, unidadDeTrabajo);

        var respuesta = await servicio.RegistrarEvaluacionAsync(SolicitudId, Evaluacion(2), CancellationToken.None);

        Assert.Equal(EstadoSolicitud.EnEvaluacion, respuesta.Estado);
        Assert.Equal(2, _almacenamiento.Nombres.Count);
        Assert.All(_almacenamiento.Nombres, nombre => Assert.StartsWith($"solicitudes/{SolicitudId}/", nombre));
        Assert.All(_almacenamiento.Nombres, nombre => Assert.EndsWith(".png", nombre));
        Assert.Equal(1, unidadDeTrabajo.Guardados);
        Assert.Contains("2 fotos", respuesta.Mensaje);
    }

    [Fact]
    public async Task RegistrarEvaluacion_PorElProveedor_NoSubeNingunArchivo()
    {
        var servicio = CrearServicio(Actores.Proveedor, new UnidadDeTrabajoFalsa());

        await Assert.ThrowsAsync<AccesoDenegadoException>(() =>
            servicio.RegistrarEvaluacionAsync(SolicitudId, Evaluacion(1), CancellationToken.None));

        Assert.Empty(_almacenamiento.Nombres);
    }

    [Fact]
    public async Task RegistrarEvaluacion_SiFallaElGuardado_EliminaLosArchivosSubidos()
    {
        var conflicto = new ConflictoConcurrenciaException(new InvalidOperationException("Otra persona evaluó primero."));
        var servicio = CrearServicio(Actores.Coordinador, new UnidadDeTrabajoFalsa(conflicto));

        await Assert.ThrowsAsync<ConflictoConcurrenciaException>(() =>
            servicio.RegistrarEvaluacionAsync(SolicitudId, Evaluacion(3), CancellationToken.None));

        Assert.Empty(_almacenamiento.Nombres);
        Assert.Equal(3, _almacenamiento.Eliminados);
    }

    [Fact]
    public async Task RegistrarEvaluacion_ConUnaFotoDeTipoNoPermitido_NoDejaArchivosSubidos()
    {
        var servicio = CrearServicio(Actores.Coordinador, new UnidadDeTrabajoFalsa());
        var fotos = new List<ArchivoFoto> { Foto("muestra.png", "image/png"), Foto("ficha.pdf", "application/pdf") };

        await Assert.ThrowsAsync<ReglaNegocioException>(() => servicio.RegistrarEvaluacionAsync(
            SolicitudId,
            new RegistrarEvaluacion(ResultadoEvaluacion.Cumple, "Conforme", fotos),
            CancellationToken.None));

        Assert.Empty(_almacenamiento.Nombres);
        Assert.Equal(EstadoSolicitud.Enviado, _solicitud.Estado);
    }

    [Fact]
    public async Task RegistrarEvaluacion_SolicitudInexistente_LanzaRecursoNoEncontrado()
    {
        var servicio = CrearServicio(Actores.Coordinador, new UnidadDeTrabajoFalsa());

        await Assert.ThrowsAsync<RecursoNoEncontradoException>(() =>
            servicio.RegistrarEvaluacionAsync(999, Evaluacion(1), CancellationToken.None));
    }

    private ServicioSolicitudes CrearServicio(Actor actor, UnidadDeTrabajoFalsa unidadDeTrabajo) => new(
        new SolicitudRepositorioEnMemoria(_solicitud),
        new ProveedorRepositorioEnMemoria(),
        new TipoMaterialRepositorioEnMemoria(),
        unidadDeTrabajo,
        new UsuarioActualFalso(actor),
        new RelojFijo(),
        _almacenamiento);

    private static RegistrarEvaluacion Evaluacion(int cantidadFotos) => new(
        ResultadoEvaluacion.Cumple,
        "Muestra conforme",
        Enumerable.Range(1, cantidadFotos).Select(numero => Foto($"muestra-{numero}.png", "image/png")).ToList());

    private static ArchivoFoto Foto(string nombre, string contentType) =>
        new(nombre, contentType, ContenidoPng.Length, () => new MemoryStream(ContenidoPng));
}
