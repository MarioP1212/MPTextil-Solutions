using MPTextil.Application.Abstracciones.Tiempo;
using MPTextil.Domain.Comun;

namespace MPTextil.Infrastructure.Tiempo;

internal sealed class RelojZonaHoraria(TimeProvider proveedorTiempo, TimeZoneInfo zonaHoraria) : IReloj
{
    public Momento Ahora()
    {
        var utc = proveedorTiempo.GetUtcNow();
        var local = TimeZoneInfo.ConvertTime(utc, zonaHoraria);

        return new Momento(utc.UtcDateTime, DateOnly.FromDateTime(local.DateTime));
    }
}
