using MPTextil.Domain.Enumeraciones;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Dominio;

public sealed class SolicitudAccionesDisponiblesTests
{
    [Fact]
    public void Solicitada_SoloElCoordinadorPuedeAsignar()
    {
        var solicitud = Escenarios.Solicitada();

        Assert.Equal([AccionSolicitud.Asignar], solicitud.AccionesDisponibles(Actores.Coordinador));
        Assert.Empty(solicitud.AccionesDisponibles(Actores.Solicitante));
        Assert.Empty(solicitud.AccionesDisponibles(Actores.Proveedor));
        Assert.Empty(solicitud.AccionesDisponibles(Actores.Autorizador));
    }

    [Fact]
    public void Asignada_ElProveedorAsignadoPuedeDeclinarOAceptar()
    {
        var solicitud = Escenarios.Asignada();

        Assert.Equal([AccionSolicitud.Declinar, AccionSolicitud.Aceptar], solicitud.AccionesDisponibles(Actores.Proveedor));
        Assert.Empty(solicitud.AccionesDisponibles(Actores.OtroProveedor));
    }

    [Fact]
    public void EnProgresoSinRechazos_ElProveedorSoloPuedeEnviar()
    {
        var solicitud = Escenarios.EnProgreso();

        Assert.Equal([AccionSolicitud.RegistrarEnvio], solicitud.AccionesDisponibles(Actores.Proveedor));
    }

    [Fact]
    public void EnProgresoTrasRechazo_ElProveedorPuedeCambiarFechaOEnviar()
    {
        var solicitud = Escenarios.RechazadaEnDia(0);

        Assert.Equal(
            [AccionSolicitud.CambiarFechaCompromiso, AccionSolicitud.RegistrarEnvio],
            solicitud.AccionesDisponibles(Actores.Proveedor));
    }

    [Fact]
    public void EnProgresoSinCompromisoVigente_ElProveedorSoloPuedeRegistrarFecha()
    {
        var solicitud = Escenarios.RechazadaEnDia(DatosPrueba.DiasCompromiso + 1);

        Assert.Equal([AccionSolicitud.CambiarFechaCompromiso], solicitud.AccionesDisponibles(Actores.Proveedor));
    }

    [Fact]
    public void Enviada_SoloElCoordinadorPuedeEvaluar()
    {
        var solicitud = Escenarios.Enviada();

        Assert.Equal([AccionSolicitud.RegistrarEvaluacion], solicitud.AccionesDisponibles(Actores.Coordinador));
        Assert.Empty(solicitud.AccionesDisponibles(Actores.Proveedor));
    }

    [Fact]
    public void EnEvaluacion_SoloElSolicitanteCreadorPuedeDecidir()
    {
        var solicitud = Escenarios.EnEvaluacion();

        Assert.Equal([AccionSolicitud.Aprobar, AccionSolicitud.Rechazar], solicitud.AccionesDisponibles(Actores.Solicitante));
        Assert.Empty(solicitud.AccionesDisponibles(Actores.OtroSolicitante));
        Assert.Empty(solicitud.AccionesDisponibles(Actores.Autorizador));
    }

    [Fact]
    public void PendienteDeAutorizacion_SoloElAutorizadorPuedeDecidir()
    {
        var solicitud = Escenarios.PendienteDeAutorizacion();

        Assert.Equal([AccionSolicitud.Autorizar, AccionSolicitud.Rechazar], solicitud.AccionesDisponibles(Actores.Autorizador));
        Assert.Empty(solicitud.AccionesDisponibles(Actores.Solicitante));
    }

    [Fact]
    public void Aprobada_NadiePuedeHacerMasAcciones()
    {
        var solicitud = Escenarios.Aprobada();

        Assert.Empty(solicitud.AccionesDisponibles(Actores.Solicitante));
        Assert.Empty(solicitud.AccionesDisponibles(Actores.Coordinador));
        Assert.Empty(solicitud.AccionesDisponibles(Actores.Proveedor));
        Assert.Empty(solicitud.AccionesDisponibles(Actores.Autorizador));
    }
}
