namespace MPTextil.Web.Modelos;

public sealed record SolicitudResumen(
    int Id,
    string Codigo,
    string NombreMaterial,
    string TipoMaterial,
    Prioridad Prioridad,
    string PrioridadNombre,
    EstadoSolicitud Estado,
    string EstadoNombre,
    string Solicitante,
    string? Proveedor,
    DateOnly FechaObjetivo,
    DateOnly? FechaCompromiso,
    bool EstaVencida,
    DateTime FechaActualizacion);

public sealed record SolicitudDetalle(
    int Id,
    string Codigo,
    string NombreMaterial,
    string Descripcion,
    string TipoMaterial,
    Prioridad Prioridad,
    string PrioridadNombre,
    EstadoSolicitud Estado,
    string EstadoNombre,
    string Solicitante,
    int? ProveedorId,
    string? Proveedor,
    DateOnly FechaObjetivo,
    DateOnly? FechaCompromiso,
    bool EstaVencida,
    bool RequiereNuevaFechaCompromiso,
    DateTime FechaCreacion,
    DateTime FechaActualizacion,
    IReadOnlyList<AccionSolicitud> AccionesDisponibles,
    IReadOnlyList<EntregaDetalle> Entregas,
    IReadOnlyList<HistorialDetalle> Historial);

public sealed record EntregaDetalle(
    int CompromisoId,
    DateOnly FechaCompromiso,
    EstadoCompromiso Estado,
    string EstadoNombre,
    string RegistradoPor,
    DateTime FechaRegistro,
    DateOnly? FechaReemplazo,
    EnvioDetalle? Envio);

public sealed record EnvioDetalle(
    DateOnly FechaEnvio,
    string Transportista,
    string NumeroGuia,
    string RegistradoPor,
    EvaluacionDetalle? Evaluacion);

public sealed record EvaluacionDetalle(
    ResultadoEvaluacion Resultado,
    string ResultadoNombre,
    string Comentarios,
    string EvaluadoPor,
    DateTime FechaEvaluacion,
    IReadOnlyList<FotoDetalle> Fotos,
    IReadOnlyList<DecisionDetalle> Decisiones);

public sealed record FotoDetalle(int Id, string NombreOriginal, string ContentType, long TamanoBytes, string Url);

public sealed record DecisionDetalle(NivelDecision Nivel, string NivelNombre, bool Aprobada, string? Motivo, string Usuario, DateTime Fecha);

public sealed record HistorialDetalle(
    DateTime Fecha,
    string Usuario,
    AccionSolicitud Accion,
    string AccionNombre,
    EstadoSolicitud? EstadoAnterior,
    string? EstadoAnteriorNombre,
    EstadoSolicitud EstadoNuevo,
    string EstadoNuevoNombre,
    string Detalle);

public sealed record RespuestaAccion(int SolicitudId, string Codigo, EstadoSolicitud Estado, string EstadoNombre, string Mensaje);

public sealed record CrearSolicitud(string NombreMaterial, string Descripcion, int TipoMaterialId, Prioridad Prioridad, DateOnly FechaObjetivo);

public sealed record AsignarProveedor(int ProveedorId);

public sealed record MotivoAccion(string Motivo);

public sealed record FechaCompromisoSolicitud(DateOnly FechaCompromiso);

public sealed record RegistrarEnvioSolicitud(DateOnly FechaEnvio, string Transportista, string NumeroGuia);
