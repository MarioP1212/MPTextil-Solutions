using MPTextil.Infrastructure.Tiempo;
using MPTextil.Tests.Infraestructura.Soporte;

namespace MPTextil.Tests.Infraestructura;

public sealed class RelojZonaHorariaTests
{
    private static readonly TimeZoneInfo Honduras = TimeZoneInfo.FindSystemTimeZoneById("America/Tegucigalpa");

    [Fact]
    public void Ahora_DeNocheEnHondurasYaEsOtroDiaEnUtc_DevuelveLaFechaLocal()
    {
        var utc = new DateTimeOffset(2026, 11, 11, 3, 0, 0, TimeSpan.Zero);
        var reloj = new RelojZonaHoraria(new TiempoFijo(utc), Honduras);

        var momento = reloj.Ahora();

        Assert.Equal(new DateOnly(2026, 11, 10), momento.Hoy);
        Assert.Equal(utc.UtcDateTime, momento.Utc);
        Assert.Equal(DateTimeKind.Utc, momento.Utc.Kind);
    }

    [Fact]
    public void Ahora_PasadaLaMedianocheEnHonduras_DevuelveElNuevoDia()
    {
        var utc = new DateTimeOffset(2026, 11, 11, 6, 30, 0, TimeSpan.Zero);
        var reloj = new RelojZonaHoraria(new TiempoFijo(utc), Honduras);

        var momento = reloj.Ahora();

        Assert.Equal(new DateOnly(2026, 11, 11), momento.Hoy);
    }
}
