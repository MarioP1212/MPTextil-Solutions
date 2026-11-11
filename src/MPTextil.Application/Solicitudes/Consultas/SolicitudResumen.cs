using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Application.Solicitudes.Consultas;

public sealed record SolicitudResumen(
    int Id,
    string Codigo,
    string NombreMaterial,
    string TipoMaterial,
    Prioridad Prioridad,
    EstadoSolicitud Estado,
    string Solicitante,
    string? Proveedor,
    DateOnly FechaObjetivo,
    DateOnly? FechaCompromiso,
    bool EstaVencida,
    DateTime FechaActualizacion)
{
    public string PrioridadNombre => Prioridad.Descripcion();
    public string EstadoNombre => Estado.Descripcion();
}
