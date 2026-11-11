using MPTextil.Application.Solicitudes.Consultas;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Application.Abstracciones.Consultas;

public interface IConsultasSolicitudes
{
    Task<IReadOnlyList<SolicitudResumen>> ListarAsync(FiltroSolicitudes filtro, CancellationToken cancellationToken);

    Task<DatosDetalleSolicitud?> ObtenerDetalleAsync(int id, CancellationToken cancellationToken);
}

public sealed record FiltroSolicitudes(
    DateOnly Hoy,
    int? ProveedorId,
    EstadoSolicitud? Estado = null,
    bool SoloVencidas = false);

public sealed record DatosDetalleSolicitud(Solicitud Solicitud, IReadOnlyDictionary<int, string> NombresUsuarios);
