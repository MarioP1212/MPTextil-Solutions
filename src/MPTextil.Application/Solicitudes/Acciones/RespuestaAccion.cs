using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Application.Solicitudes.Acciones;

public sealed record RespuestaAccion(int SolicitudId, string Codigo, EstadoSolicitud Estado, string Mensaje)
{
    public string EstadoNombre => Estado.Descripcion();

    internal static RespuestaAccion Desde(Solicitud solicitud, string mensaje) =>
        new(solicitud.Id, solicitud.Codigo, solicitud.Estado, mensaje);
}
