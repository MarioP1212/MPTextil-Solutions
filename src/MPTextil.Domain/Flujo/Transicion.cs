using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Domain.Flujo;

public sealed record Transicion(AccionSolicitud Accion, EstadoSolicitud Origen, EstadoSolicitud Destino, Rol Rol);
