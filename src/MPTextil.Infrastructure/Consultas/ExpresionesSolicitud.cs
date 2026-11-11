using System.Linq.Expressions;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Infrastructure.Consultas;

internal static class ExpresionesSolicitud
{
    public static Expression<Func<Solicitud, bool>> EstaVencida(DateOnly hoy) => solicitud =>
        solicitud.Estado == EstadoSolicitud.EnProgreso
        && solicitud.Compromisos.Any(compromiso =>
            compromiso.FechaReemplazo == null && compromiso.Envio == null && compromiso.FechaCompromiso < hoy);
}
