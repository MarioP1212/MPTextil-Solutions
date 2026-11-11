using Microsoft.EntityFrameworkCore;
using MPTextil.Application.Abstracciones.Persistencia;
using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Persistencia.Repositorios;

internal sealed class SolicitudRepositorio(MPTextilDbContext contexto)
    : RepositorioBase<Solicitud>(contexto), ISolicitudRepositorio
{
    public override Task<Solicitud?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        Conjunto
            .Include(solicitud => solicitud.Compromisos)
                .ThenInclude(compromiso => compromiso.Envio!)
                .ThenInclude(envio => envio.Evaluacion!)
                .ThenInclude(evaluacion => evaluacion.Decisiones)
            .AsSplitQuery()
            .FirstOrDefaultAsync(solicitud => solicitud.Id == id, cancellationToken);
}
