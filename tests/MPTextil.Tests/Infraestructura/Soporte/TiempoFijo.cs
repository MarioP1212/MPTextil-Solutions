namespace MPTextil.Tests.Infraestructura.Soporte;

internal sealed class TiempoFijo(DateTimeOffset utc) : TimeProvider
{
    public override DateTimeOffset GetUtcNow() => utc;
}
