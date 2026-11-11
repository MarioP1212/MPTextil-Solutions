using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Domain.Flujo;

public static class FlujoSolicitud
{
    private static readonly Transicion[] Transiciones =
    [
        new(AccionSolicitud.Asignar, EstadoSolicitud.Solicitado, EstadoSolicitud.Asignado, Rol.Coordinador),
        new(AccionSolicitud.Declinar, EstadoSolicitud.Asignado, EstadoSolicitud.Solicitado, Rol.Proveedor),
        new(AccionSolicitud.Aceptar, EstadoSolicitud.Asignado, EstadoSolicitud.EnProgreso, Rol.Proveedor),
        new(AccionSolicitud.CambiarFechaCompromiso, EstadoSolicitud.EnProgreso, EstadoSolicitud.EnProgreso, Rol.Proveedor),
        new(AccionSolicitud.RegistrarEnvio, EstadoSolicitud.EnProgreso, EstadoSolicitud.Enviado, Rol.Proveedor),
        new(AccionSolicitud.RegistrarEvaluacion, EstadoSolicitud.Enviado, EstadoSolicitud.EnEvaluacion, Rol.Coordinador),
        new(AccionSolicitud.Aprobar, EstadoSolicitud.EnEvaluacion, EstadoSolicitud.PendienteAutorizacion, Rol.Solicitante),
        new(AccionSolicitud.Rechazar, EstadoSolicitud.EnEvaluacion, EstadoSolicitud.EnProgreso, Rol.Solicitante),
        new(AccionSolicitud.Autorizar, EstadoSolicitud.PendienteAutorizacion, EstadoSolicitud.Aprobado, Rol.Autorizador),
        new(AccionSolicitud.Rechazar, EstadoSolicitud.PendienteAutorizacion, EstadoSolicitud.EnProgreso, Rol.Autorizador)
    ];

    public static bool RolPuedeEjecutar(Rol rol, AccionSolicitud accion) =>
        Transiciones.Any(transicion => transicion.Rol == rol && transicion.Accion == accion);

    public static Transicion? Buscar(EstadoSolicitud origen, AccionSolicitud accion, Rol rol) =>
        Transiciones.FirstOrDefault(transicion =>
            transicion.Origen == origen && transicion.Accion == accion && transicion.Rol == rol);

    public static IEnumerable<Transicion> DisponiblesDesde(EstadoSolicitud origen, Rol rol) =>
        Transiciones.Where(transicion => transicion.Origen == origen && transicion.Rol == rol);
}
