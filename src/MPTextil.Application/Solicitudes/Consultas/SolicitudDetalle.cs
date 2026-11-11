using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Application.Solicitudes.Consultas;

public sealed record SolicitudDetalle(
    int Id,
    string Codigo,
    string NombreMaterial,
    string Descripcion,
    string TipoMaterial,
    Prioridad Prioridad,
    EstadoSolicitud Estado,
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
    IReadOnlyList<HistorialDetalle> Historial)
{
    public string PrioridadNombre => Prioridad.Descripcion();
    public string EstadoNombre => Estado.Descripcion();
}

public sealed record EntregaDetalle(
    int CompromisoId,
    DateOnly FechaCompromiso,
    EstadoCompromiso Estado,
    string RegistradoPor,
    DateTime FechaRegistro,
    DateOnly? FechaReemplazo,
    EnvioDetalle? Envio)
{
    public string EstadoNombre => Estado.Descripcion();
}

public sealed record EnvioDetalle(
    DateOnly FechaEnvio,
    string Transportista,
    string NumeroGuia,
    string RegistradoPor,
    EvaluacionDetalle? Evaluacion);

public sealed record EvaluacionDetalle(
    ResultadoEvaluacion Resultado,
    string Comentarios,
    string EvaluadoPor,
    DateTime FechaEvaluacion,
    IReadOnlyList<FotoDetalle> Fotos,
    IReadOnlyList<DecisionDetalle> Decisiones)
{
    public string ResultadoNombre => Resultado.Descripcion();
}

public sealed record FotoDetalle(int Id, string NombreOriginal, string ContentType, long TamanoBytes, string Url);

public sealed record DecisionDetalle(NivelDecision Nivel, bool Aprobada, string? Motivo, string Usuario, DateTime Fecha)
{
    public string NivelNombre => Nivel.Descripcion();
}

public sealed record HistorialDetalle(
    DateTime Fecha,
    string Usuario,
    AccionSolicitud Accion,
    EstadoSolicitud? EstadoAnterior,
    EstadoSolicitud EstadoNuevo,
    string Detalle)
{
    public string AccionNombre => Accion.Titulo();
    public string? EstadoAnteriorNombre => EstadoAnterior?.Descripcion();
    public string EstadoNuevoNombre => EstadoNuevo.Descripcion();
}
