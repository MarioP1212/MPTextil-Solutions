using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Domain.Excepciones;

public sealed class TransicionInvalidaException(AccionSolicitud accion, EstadoSolicitud estado)
    : DominioException($"No es posible {accion.Descripcion()} en estado {estado.Descripcion()}.");
