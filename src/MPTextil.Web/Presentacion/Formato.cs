using System.Globalization;

namespace MPTextil.Web.Presentacion;

public static class Formato
{
    private const long BytesPorKilobyte = 1024;

    public static string Fecha(DateOnly fecha) => fecha.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

    public static string Fecha(DateOnly? fecha) => fecha is { } valor ? Fecha(valor) : "Sin fecha";

    public static string FechaHora(DateTime fechaUtc) =>
        DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc).ToLocalTime().ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

    public static string Tamano(long bytes) => $"{Math.Max(1, bytes / BytesPorKilobyte):N0} KB";

    public static string Porcentaje(decimal? valor) =>
        valor is { } porcentaje ? $"{porcentaje.ToString("0.#", CultureInfo.InvariantCulture)}%" : "Sin entregas";
}
