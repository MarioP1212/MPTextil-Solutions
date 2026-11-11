namespace MPTextil.Web.Modelos;

public enum Rol
{
    Solicitante = 1,
    Coordinador = 2,
    Proveedor = 3,
    Autorizador = 4
}

public enum EstadoSolicitud
{
    Solicitado = 1,
    Asignado = 2,
    EnProgreso = 3,
    Enviado = 4,
    EnEvaluacion = 5,
    PendienteAutorizacion = 6,
    Aprobado = 7
}

public enum Prioridad
{
    Baja = 1,
    Media = 2,
    Alta = 3,
    Urgente = 4
}

public enum ResultadoEvaluacion
{
    Cumple = 1,
    NoCumple = 2,
    CumpleConObservaciones = 3
}

public enum NivelDecision
{
    Aprobacion = 1,
    Autorizacion = 2
}

public enum AccionSolicitud
{
    Crear = 1,
    Asignar = 2,
    Declinar = 3,
    Aceptar = 4,
    CambiarFechaCompromiso = 5,
    RegistrarEnvio = 6,
    RegistrarEvaluacion = 7,
    Aprobar = 8,
    Autorizar = 9,
    Rechazar = 10
}

public enum EstadoCompromiso
{
    Vigente = 1,
    Vencido = 2,
    EntregadoATiempo = 3,
    EntregadoTarde = 4,
    Reemplazado = 5,
    ReemplazadoVencido = 6
}
