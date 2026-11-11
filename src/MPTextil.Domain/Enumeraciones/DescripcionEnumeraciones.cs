namespace MPTextil.Domain.Enumeraciones;

public static class DescripcionEnumeraciones
{
    public static string Descripcion(this EstadoSolicitud estado) => estado switch
    {
        EstadoSolicitud.Solicitado => "Solicitado",
        EstadoSolicitud.Asignado => "Asignado",
        EstadoSolicitud.EnProgreso => "En progreso",
        EstadoSolicitud.Enviado => "Enviado",
        EstadoSolicitud.EnEvaluacion => "En evaluación",
        EstadoSolicitud.PendienteAutorizacion => "Pendiente de autorización",
        EstadoSolicitud.Aprobado => "Aprobado",
        _ => estado.ToString()
    };

    public static string Descripcion(this Rol rol) => rol switch
    {
        Rol.Solicitante => "Solicitante",
        Rol.Coordinador => "Coordinador",
        Rol.Proveedor => "Proveedor",
        Rol.Autorizador => "Autorizador",
        _ => rol.ToString()
    };

    public static string Descripcion(this Prioridad prioridad) => prioridad switch
    {
        Prioridad.Baja => "Baja",
        Prioridad.Media => "Media",
        Prioridad.Alta => "Alta",
        Prioridad.Urgente => "Urgente",
        _ => prioridad.ToString()
    };

    public static string Descripcion(this ResultadoEvaluacion resultado) => resultado switch
    {
        ResultadoEvaluacion.Cumple => "Cumple",
        ResultadoEvaluacion.NoCumple => "No cumple",
        ResultadoEvaluacion.CumpleConObservaciones => "Cumple con observaciones",
        _ => resultado.ToString()
    };

    public static string Descripcion(this NivelDecision nivel) => nivel switch
    {
        NivelDecision.Aprobacion => "Aprobación",
        NivelDecision.Autorizacion => "Autorización",
        _ => nivel.ToString()
    };

    public static string Descripcion(this AccionSolicitud accion) => accion switch
    {
        AccionSolicitud.Crear => "crear la solicitud",
        AccionSolicitud.Asignar => "asignar la solicitud",
        AccionSolicitud.Declinar => "declinar la asignación",
        AccionSolicitud.Aceptar => "aceptar la solicitud",
        AccionSolicitud.CambiarFechaCompromiso => "cambiar la fecha compromiso",
        AccionSolicitud.RegistrarEnvio => "registrar el envío",
        AccionSolicitud.RegistrarEvaluacion => "registrar la evaluación",
        AccionSolicitud.Aprobar => "aprobar la muestra",
        AccionSolicitud.Autorizar => "autorizar la muestra",
        AccionSolicitud.Rechazar => "rechazar la muestra",
        _ => accion.ToString()
    };

    public static string Titulo(this AccionSolicitud accion) => accion switch
    {
        AccionSolicitud.Crear => "Solicitud creada",
        AccionSolicitud.Asignar => "Asignada a proveedor",
        AccionSolicitud.Declinar => "Asignación declinada",
        AccionSolicitud.Aceptar => "Aceptada por el proveedor",
        AccionSolicitud.CambiarFechaCompromiso => "Fecha compromiso actualizada",
        AccionSolicitud.RegistrarEnvio => "Muestra enviada",
        AccionSolicitud.RegistrarEvaluacion => "Muestra evaluada",
        AccionSolicitud.Aprobar => "Muestra aprobada",
        AccionSolicitud.Autorizar => "Muestra autorizada",
        AccionSolicitud.Rechazar => "Muestra rechazada",
        _ => accion.ToString()
    };
}
