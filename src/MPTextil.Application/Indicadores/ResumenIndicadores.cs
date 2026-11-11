using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Application.Indicadores;

public sealed record ResumenIndicadores(
    DateOnly Fecha,
    int TotalSolicitudes,
    int SolicitudesVencidas,
    decimal? PorcentajeATiempoGeneral,
    IReadOnlyList<ConteoEstado> SolicitudesPorEstado,
    IReadOnlyList<CumplimientoProveedor> CumplimientoPorProveedor);

public sealed record ConteoEstado(EstadoSolicitud Estado, int Cantidad)
{
    public string EstadoNombre => Estado.Descripcion();
}

public sealed record CumplimientoProveedor(
    int ProveedorId,
    string Proveedor,
    int EntregasATiempo,
    int EntregasTarde,
    int VencidosSinEnvio,
    decimal? PorcentajeATiempo)
{
    public int TotalMedido => EntregasATiempo + EntregasTarde + VencidosSinEnvio;
}
