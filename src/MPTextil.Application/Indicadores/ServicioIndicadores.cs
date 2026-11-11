using MPTextil.Application.Abstracciones.Consultas;
using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Application.Abstracciones.Tiempo;
using MPTextil.Application.Solicitudes;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Application.Indicadores;

public sealed class ServicioIndicadores(
    IConsultasIndicadores consultas,
    IUsuarioActual usuarioActual,
    IReloj reloj)
{
    private const int DecimalesPorcentaje = 1;

    public async Task<ResumenIndicadores> ObtenerAsync(CancellationToken cancellationToken)
    {
        var hoy = reloj.Ahora().Hoy;
        var proveedorId = VisibilidadSolicitudes.ProveedorVisible(usuarioActual.Actor);
        var datos = await consultas.ObtenerAsync(hoy, proveedorId, cancellationToken);

        var porEstado = Enum.GetValues<EstadoSolicitud>()
            .Select(estado => new ConteoEstado(estado, datos.SolicitudesPorEstado.GetValueOrDefault(estado)))
            .ToList();

        var cumplimiento = datos.EntregasPorProveedor
            .Select(entregas => new CumplimientoProveedor(
                entregas.ProveedorId,
                entregas.Proveedor,
                entregas.ATiempo,
                entregas.Tarde,
                entregas.VencidosSinEnvio,
                Porcentaje(entregas.ATiempo, entregas.ATiempo + entregas.Tarde + entregas.VencidosSinEnvio)))
            .OrderByDescending(proveedor => proveedor.PorcentajeATiempo)
            .ThenBy(proveedor => proveedor.Proveedor)
            .ToList();

        return new ResumenIndicadores(
            hoy,
            porEstado.Sum(conteo => conteo.Cantidad),
            datos.SolicitudesVencidas,
            Porcentaje(cumplimiento.Sum(proveedor => proveedor.EntregasATiempo), cumplimiento.Sum(proveedor => proveedor.TotalMedido)),
            porEstado,
            cumplimiento);
    }

    private static decimal? Porcentaje(int aTiempo, int medidos) =>
        medidos == 0 ? null : Math.Round(100m * aTiempo / medidos, DecimalesPorcentaje, MidpointRounding.AwayFromZero);
}
