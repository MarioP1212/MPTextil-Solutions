namespace MPTextil.Application.Excepciones;

public sealed class ValidacionException(IDictionary<string, string[]> errores)
    : Exception("Algunos datos no son válidos. Revisa los campos marcados.")
{
    public IDictionary<string, string[]> Errores { get; } = errores;

    public static ValidacionException DeCampo(string campo, string mensaje) =>
        new(new Dictionary<string, string[]> { [campo] = [mensaje] });
}
