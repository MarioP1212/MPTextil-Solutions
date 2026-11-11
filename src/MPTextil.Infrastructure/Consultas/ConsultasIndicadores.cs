using Microsoft.EntityFrameworkCore;
using MPTextil.Application.Abstracciones.Consultas;
using MPTextil.Domain.Entidades;
using MPTextil.Infrastructure.Persistencia;

namespace MPTextil.Infrastructure.Consultas;

internal sealed class ConsultasIndicadores(MPTextilDbContext contexto) : IConsultasIndicadores
{
    public async Task<DatosIndicadores> ObtenerAsync(DateOnly hoy, int? proveedorId, CancellationToken cancellationToken)
    {
        var solicitudes = contexto.Solicitudes.AsNoTracking();
        var proveedores = contexto.Proveedores.AsNoTracking();
        var compromisos = contexto.Set<CompromisoEntrega>().AsNoTracking();

        if (proveedorId is { } id)
        {
            solicitudes = solicitudes.Where(solicitud => solicitud.ProveedorId == id);
            proveedores = proveedores.Where(proveedor => proveedor.Id == id);
            compromisos = compromisos.Where(compromiso => compromiso.ProveedorId == id);
        }

        var porEstado = await solicitudes
            .GroupBy(solicitud => solicitud.Estado)
            .Select(grupo => new { Estado = grupo.Key, Cantidad = grupo.Count() })
            .ToDictionaryAsync(conteo => conteo.Estado, conteo => conteo.Cantidad, cancellationToken);

        var vencidas = await solicitudes.CountAsync(ExpresionesSolicitud.EstaVencida(hoy), cancellationToken);

        var conteos = await compromisos
            .Select(compromiso => new
            {
                compromiso.ProveedorId,
                compromiso.FechaCompromiso,
                compromiso.FechaReemplazo,
                FechaEnvio = compromiso.Envio == null ? (DateOnly?)null : compromiso.Envio.FechaEnvio
            })
            .GroupBy(compromiso => compromiso.ProveedorId)
            .Select(grupo => new
            {
                ProveedorId = grupo.Key,
                ATiempo = grupo.Count(compromiso => compromiso.FechaEnvio != null && compromiso.FechaEnvio <= compromiso.FechaCompromiso),
                Tarde = grupo.Count(compromiso => compromiso.FechaEnvio != null && compromiso.FechaEnvio > compromiso.FechaCompromiso),
                VencidosSinEnvio = grupo.Count(compromiso =>
                    compromiso.FechaEnvio == null && compromiso.FechaCompromiso < (compromiso.FechaReemplazo ?? hoy))
            })
            .ToDictionaryAsync(conteo => conteo.ProveedorId, cancellationToken);

        var entregas = await proveedores
            .OrderBy(proveedor => proveedor.Nombre)
            .Select(proveedor => new { proveedor.Id, proveedor.Nombre })
            .ToListAsync(cancellationToken);

        return new DatosIndicadores(
            porEstado,
            vencidas,
            entregas
                .Select(proveedor => conteos.TryGetValue(proveedor.Id, out var conteo)
                    ? new EntregasProveedor(proveedor.Id, proveedor.Nombre, conteo.ATiempo, conteo.Tarde, conteo.VencidosSinEnvio)
                    : new EntregasProveedor(proveedor.Id, proveedor.Nombre, 0, 0, 0))
                .ToList());
    }
}
