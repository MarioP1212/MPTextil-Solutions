using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Flujo;

namespace MPTextil.Tests.Dominio;

public sealed class FlujoSolicitudTests
{
    [Theory]
    [InlineData(EstadoSolicitud.Solicitado, AccionSolicitud.Asignar, Rol.Coordinador, EstadoSolicitud.Asignado)]
    [InlineData(EstadoSolicitud.Asignado, AccionSolicitud.Declinar, Rol.Proveedor, EstadoSolicitud.Solicitado)]
    [InlineData(EstadoSolicitud.Asignado, AccionSolicitud.Aceptar, Rol.Proveedor, EstadoSolicitud.EnProgreso)]
    [InlineData(EstadoSolicitud.EnProgreso, AccionSolicitud.CambiarFechaCompromiso, Rol.Proveedor, EstadoSolicitud.EnProgreso)]
    [InlineData(EstadoSolicitud.EnProgreso, AccionSolicitud.RegistrarEnvio, Rol.Proveedor, EstadoSolicitud.Enviado)]
    [InlineData(EstadoSolicitud.Enviado, AccionSolicitud.RegistrarEvaluacion, Rol.Coordinador, EstadoSolicitud.EnEvaluacion)]
    [InlineData(EstadoSolicitud.EnEvaluacion, AccionSolicitud.Aprobar, Rol.Solicitante, EstadoSolicitud.PendienteAutorizacion)]
    [InlineData(EstadoSolicitud.EnEvaluacion, AccionSolicitud.Rechazar, Rol.Solicitante, EstadoSolicitud.EnProgreso)]
    [InlineData(EstadoSolicitud.PendienteAutorizacion, AccionSolicitud.Autorizar, Rol.Autorizador, EstadoSolicitud.Aprobado)]
    [InlineData(EstadoSolicitud.PendienteAutorizacion, AccionSolicitud.Rechazar, Rol.Autorizador, EstadoSolicitud.EnProgreso)]
    public void Buscar_TransicionPermitida_DevuelveElEstadoDestino(
        EstadoSolicitud origen,
        AccionSolicitud accion,
        Rol rol,
        EstadoSolicitud destinoEsperado)
    {
        var transicion = FlujoSolicitud.Buscar(origen, accion, rol);

        Assert.NotNull(transicion);
        Assert.Equal(destinoEsperado, transicion.Destino);
    }

    [Theory]
    [InlineData(EstadoSolicitud.Solicitado, AccionSolicitud.Aceptar, Rol.Proveedor)]
    [InlineData(EstadoSolicitud.Asignado, AccionSolicitud.Asignar, Rol.Coordinador)]
    [InlineData(EstadoSolicitud.Enviado, AccionSolicitud.RegistrarEvaluacion, Rol.Solicitante)]
    [InlineData(EstadoSolicitud.EnEvaluacion, AccionSolicitud.Autorizar, Rol.Autorizador)]
    [InlineData(EstadoSolicitud.PendienteAutorizacion, AccionSolicitud.Rechazar, Rol.Solicitante)]
    [InlineData(EstadoSolicitud.Aprobado, AccionSolicitud.Rechazar, Rol.Autorizador)]
    public void Buscar_TransicionNoPermitida_DevuelveNulo(EstadoSolicitud origen, AccionSolicitud accion, Rol rol)
    {
        var transicion = FlujoSolicitud.Buscar(origen, accion, rol);

        Assert.Null(transicion);
    }

    [Theory]
    [InlineData(Rol.Coordinador, AccionSolicitud.Asignar, true)]
    [InlineData(Rol.Proveedor, AccionSolicitud.RegistrarEnvio, true)]
    [InlineData(Rol.Solicitante, AccionSolicitud.Rechazar, true)]
    [InlineData(Rol.Autorizador, AccionSolicitud.Rechazar, true)]
    [InlineData(Rol.Proveedor, AccionSolicitud.Aprobar, false)]
    [InlineData(Rol.Coordinador, AccionSolicitud.Rechazar, false)]
    [InlineData(Rol.Solicitante, AccionSolicitud.Autorizar, false)]
    public void RolPuedeEjecutar_SegunLaTabla_DevuelveElPermisoEsperado(Rol rol, AccionSolicitud accion, bool esperado)
    {
        var puede = FlujoSolicitud.RolPuedeEjecutar(rol, accion);

        Assert.Equal(esperado, puede);
    }

    [Theory]
    [InlineData(Rol.Solicitante)]
    [InlineData(Rol.Coordinador)]
    [InlineData(Rol.Proveedor)]
    [InlineData(Rol.Autorizador)]
    public void DisponiblesDesde_SolicitudAprobada_NoTieneTransiciones(Rol rol)
    {
        var transiciones = FlujoSolicitud.DisponiblesDesde(EstadoSolicitud.Aprobado, rol);

        Assert.Empty(transiciones);
    }
}
