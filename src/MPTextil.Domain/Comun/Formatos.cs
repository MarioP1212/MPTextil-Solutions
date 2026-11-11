using System.Globalization;

namespace MPTextil.Domain.Comun;

public static class Formatos
{
    private const string Fecha = "dd/MM/yyyy";

    public static string ATexto(this DateOnly fecha) => fecha.ToString(Fecha, CultureInfo.InvariantCulture);
}
