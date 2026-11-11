using Microsoft.EntityFrameworkCore;
using MPTextil.Application.Abstracciones.Consultas;
using MPTextil.Application.Solicitudes.Consultas;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Infrastructure.Persistencia;

namespace MPTextil.Infrastructure.Consultas;

internal sealed class ConsultasSolicitudes(MPTextilDbContext contexto) : IConsultasSolicitudes
{
    public async Task<IReadOnlyList<SolicitudResumen>> ListarAsync(FiltroSolicitudes filtro, CancellationToken cancellationToken)
    {
        var hoy = filtro.Hoy;
        var consulta = contexto.Solicitudes.AsNoTracking();

        if (filtro.ProveedorId is { } proveedorId)
        {
            consulta = consulta.Where(solicitud => solicitud.ProveedorId == proveedorId);
        }

        if (filtro.Estado is { } estado)
        {
            consulta = consulta.Where(solicitud => solicitud.Estado == estado);
        }

        if (filtro.SoloVencidas)
        {
            consulta = consulta.Where(ExpresionesSolicitud.EstaVencida(hoy));
        }

        var filas = await consulta
            .OrderByDescending(solicitud => solicitud.FechaActualizacion)
            .Select(solicitud => new
            {
                solicitud.Id,
                solicitud.Codigo,
                solicitud.NombreMaterial,
                TipoMaterial = solicitud.TipoMaterial!.Nombre,
                solicitud.Prioridad,
                solicitud.Estado,
                Solicitante = solicitud.Solicitante!.Nombre,
                Proveedor = solicitud.Proveedor != null ? solicitud.Proveedor.Nombre : null,
                solicitud.FechaObjetivo,
                FechaCompromiso = solicitud.Compromisos
                    .Where(compromiso => compromiso.FechaReemplazo == null && compromiso.Envio == null)
                    .Select(compromiso => (DateOnly?)compromiso.FechaCompromiso)
                    .FirstOrDefault(),
                solicitud.FechaActualizacion
            })
            .ToListAsync(cancellationToken);

        return filas
            .Select(fila => new SolicitudResumen(
                fila.Id,
                fila.Codigo,
                fila.NombreMaterial,
                fila.TipoMaterial,
                fila.Prioridad,
                fila.Estado,
                fila.Solicitante,
                fila.Proveedor,
                fila.FechaObjetivo,
                fila.FechaCompromiso,
                Solicitud.EsVencida(fila.Estado, fila.FechaCompromiso, hoy),
                fila.FechaActualizacion))
            .ToList();
    }

    public async Task<DatosDetalleSolicitud?> ObtenerDetalleAsync(int id, CancellationToken cancellationToken)
    {
        var solicitud = await contexto.Solicitudes
            .AsNoTracking()
            .Include(solicitud => solicitud.TipoMaterial)
            .Include(solicitud => solicitud.Solicitante)
            .Include(solicitud => solicitud.Proveedor)
            .Include(solicitud => solicitud.Historial)
                .ThenInclude(registro => registro.Usuario)
            .Include(solicitud => solicitud.Compromisos)
                .ThenInclude(compromiso => compromiso.Envio!)
                .ThenInclude(envio => envio.Evaluacion!)
                .ThenInclude(evaluacion => evaluacion.Fotos)
            .Include(solicitud => solicitud.Compromisos)
                .ThenInclude(compromiso => compromiso.Envio!)
                .ThenInclude(envio => envio.Evaluacion!)
                .ThenInclude(evaluacion => evaluacion.Decisiones)
            .AsSplitQuery()
            .FirstOrDefaultAsync(solicitud => solicitud.Id == id, cancellationToken);

        if (solicitud is null)
        {
            return null;
        }

        var idsUsuarios = solicitud.Compromisos.SelectMany(UsuariosQueParticiparon).Distinct().ToList();
        var nombres = await contexto.Usuarios
            .AsNoTracking()
            .Where(usuario => idsUsuarios.Contains(usuario.Id))
            .ToDictionaryAsync(usuario => usuario.Id, usuario => usuario.Nombre, cancellationToken);

        return new DatosDetalleSolicitud(solicitud, nombres);
    }

    private static IEnumerable<int> UsuariosQueParticiparon(CompromisoEntrega compromiso)
    {
        yield return compromiso.RegistradoPorId;

        if (compromiso.Envio is not { } envio)
        {
            yield break;
        }

        yield return envio.RegistradoPorId;

        if (envio.Evaluacion is not { } evaluacion)
        {
            yield break;
        }

        yield return evaluacion.EvaluadoPorId;

        foreach (var decision in evaluacion.Decisiones)
        {
            yield return decision.UsuarioId;
        }
    }
}
