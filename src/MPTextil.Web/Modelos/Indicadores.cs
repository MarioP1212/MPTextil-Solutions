namespace MPTextil.Web.Modelos;

public sealed record ResumenIndicadores(
    DateOnly Fecha,
    int TotalSolicitudes,
    int SolicitudesVencidas,
    decimal? PorcentajeATiempoGeneral,
    IReadOnlyList<ConteoEstado> SolicitudesPorEstado,
    IReadOnlyList<CumplimientoProveedor> CumplimientoPorProveedor);

public sealed record ConteoEstado(EstadoSolicitud Estado, string EstadoNombre, int Cantidad);

public sealed record CumplimientoProveedor(
    int ProveedorId,
    string Proveedor,
    int EntregasATiempo,
    int EntregasTarde,
    int VencidosSinEnvio,
    decimal? PorcentajeATiempo,
    int TotalMedido);
