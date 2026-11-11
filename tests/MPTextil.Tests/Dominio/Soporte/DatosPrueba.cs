using MPTextil.Domain.Comun;
using MPTextil.Domain.Entidades;

namespace MPTextil.Tests.Dominio.Soporte;

internal static class DatosPrueba
{
    public const int TipoMaterialId = 1;
    public const int ProveedorAsignadoId = 10;
    public const int OtroProveedorId = 20;
    public const int DiasCompromiso = 5;

    public static readonly DateOnly Hoy = new(2026, 11, 10);

    private static readonly DateTime AhoraUtc = new(2026, 11, 10, 15, 0, 0, DateTimeKind.Utc);

    public static Momento EnDia(int dias = 0) => new(AhoraUtc.AddDays(dias), Hoy.AddDays(dias));

    public static TipoMaterial Tela() => TipoMaterial.Crear("Tela").ConId(TipoMaterialId);

    public static Proveedor ProveedorAsignado() =>
        Proveedor.Crear("Textiles del Norte", "ventas@textilesnorte.com", "2550-1000").ConId(ProveedorAsignadoId);

    public static Proveedor OtroProveedor() =>
        Proveedor.Crear("Hilos del Valle", "contacto@hilosdelvalle.com", null).ConId(OtroProveedorId);

    public static List<FotoEvaluacion> Fotos(int cantidad) =>
        Enumerable.Range(1, cantidad)
            .Select(numero => FotoEvaluacion.Crear($"muestras/foto-{numero}.jpg", $"foto-{numero}.jpg", "image/jpeg", 1024, AhoraUtc))
            .ToList();
}
