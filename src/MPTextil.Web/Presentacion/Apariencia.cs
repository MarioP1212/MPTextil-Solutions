using MPTextil.Web.Modelos;
using MudBlazor;

namespace MPTextil.Web.Presentacion;

public static class Apariencia
{
    public static Color ColorDe(EstadoSolicitud estado) => estado switch
    {
        EstadoSolicitud.Solicitado => Color.Default,
        EstadoSolicitud.Asignado => Color.Info,
        EstadoSolicitud.EnProgreso => Color.Primary,
        EstadoSolicitud.Enviado => Color.Secondary,
        EstadoSolicitud.EnEvaluacion => Color.Warning,
        EstadoSolicitud.PendienteAutorizacion => Color.Tertiary,
        EstadoSolicitud.Aprobado => Color.Success,
        _ => Color.Default
    };

    public static Color ColorDe(Prioridad prioridad) => prioridad switch
    {
        Prioridad.Baja => Color.Default,
        Prioridad.Media => Color.Info,
        Prioridad.Alta => Color.Warning,
        Prioridad.Urgente => Color.Error,
        _ => Color.Default
    };

    public static Color ColorDe(EstadoCompromiso estado) => estado switch
    {
        EstadoCompromiso.Vigente => Color.Primary,
        EstadoCompromiso.EntregadoATiempo => Color.Success,
        EstadoCompromiso.EntregadoTarde => Color.Warning,
        EstadoCompromiso.Vencido or EstadoCompromiso.ReemplazadoVencido => Color.Error,
        _ => Color.Default
    };

    public static Color ColorDe(ResultadoEvaluacion resultado) => resultado switch
    {
        ResultadoEvaluacion.Cumple => Color.Success,
        ResultadoEvaluacion.CumpleConObservaciones => Color.Warning,
        ResultadoEvaluacion.NoCumple => Color.Error,
        _ => Color.Default
    };

    public static Color ColorDePorcentaje(decimal? porcentaje) => porcentaje switch
    {
        null => Color.Default,
        >= 80 => Color.Success,
        >= 50 => Color.Warning,
        _ => Color.Error
    };

    public static string IconoDe(EstadoSolicitud estado) => estado switch
    {
        EstadoSolicitud.Solicitado => Icons.Material.Filled.NoteAdd,
        EstadoSolicitud.Asignado => Icons.Material.Filled.AssignmentInd,
        EstadoSolicitud.EnProgreso => Icons.Material.Filled.Construction,
        EstadoSolicitud.Enviado => Icons.Material.Filled.LocalShipping,
        EstadoSolicitud.EnEvaluacion => Icons.Material.Filled.FactCheck,
        EstadoSolicitud.PendienteAutorizacion => Icons.Material.Filled.HourglassTop,
        EstadoSolicitud.Aprobado => Icons.Material.Filled.Verified,
        _ => Icons.Material.Filled.Circle
    };

    public static string EtiquetaDe(AccionSolicitud accion) => accion switch
    {
        AccionSolicitud.Asignar => "Asignar proveedor",
        AccionSolicitud.Declinar => "Declinar",
        AccionSolicitud.Aceptar => "Aceptar solicitud",
        AccionSolicitud.CambiarFechaCompromiso => "Cambiar fecha compromiso",
        AccionSolicitud.RegistrarEnvio => "Registrar envío",
        AccionSolicitud.RegistrarEvaluacion => "Registrar evaluación",
        AccionSolicitud.Aprobar => "Aprobar muestra",
        AccionSolicitud.Autorizar => "Autorizar muestra",
        AccionSolicitud.Rechazar => "Rechazar muestra",
        _ => accion.ToString()
    };

    public static string IconoDe(AccionSolicitud accion) => accion switch
    {
        AccionSolicitud.Asignar => Icons.Material.Filled.AssignmentInd,
        AccionSolicitud.Declinar => Icons.Material.Filled.DoNotDisturbOn,
        AccionSolicitud.Aceptar => Icons.Material.Filled.EventAvailable,
        AccionSolicitud.CambiarFechaCompromiso => Icons.Material.Filled.EditCalendar,
        AccionSolicitud.RegistrarEnvio => Icons.Material.Filled.LocalShipping,
        AccionSolicitud.RegistrarEvaluacion => Icons.Material.Filled.PhotoCamera,
        AccionSolicitud.Aprobar => Icons.Material.Filled.ThumbUp,
        AccionSolicitud.Autorizar => Icons.Material.Filled.Verified,
        AccionSolicitud.Rechazar => Icons.Material.Filled.ThumbDown,
        _ => Icons.Material.Filled.PlayArrow
    };

    public static Color ColorDe(AccionSolicitud accion) => accion switch
    {
        AccionSolicitud.Declinar or AccionSolicitud.Rechazar => Color.Error,
        AccionSolicitud.Aprobar or AccionSolicitud.Autorizar => Color.Success,
        _ => Color.Primary
    };
}
