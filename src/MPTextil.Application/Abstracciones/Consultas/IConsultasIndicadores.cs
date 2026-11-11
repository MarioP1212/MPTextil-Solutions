using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Application.Abstracciones.Consultas;

public interface IConsultasIndicadores
{
    Task<DatosIndicadores> ObtenerAsync(DateOnly hoy, int? proveedorId, CancellationToken cancellationToken);
}

public sealed record DatosIndicadores(
    IReadOnlyDictionary<EstadoSolicitud, int> SolicitudesPorEstado,
    int SolicitudesVencidas,
    IReadOnlyList<EntregasProveedor> EntregasPorProveedor);

public sealed record EntregasProveedor(int ProveedorId, string Proveedor, int ATiempo, int Tarde, int VencidosSinEnvio);
