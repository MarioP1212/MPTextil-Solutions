namespace MPTextil.Web.Servicios;

public sealed class ResultadoApi<T>
{
    private static readonly IReadOnlyDictionary<string, string[]> SinErrores = new Dictionary<string, string[]>();

    private ResultadoApi(bool exito, T? datos, string mensaje, IReadOnlyDictionary<string, string[]> errores)
    {
        Exito = exito;
        Datos = datos;
        Mensaje = mensaje;
        Errores = errores;
    }

    public bool Exito { get; }
    public T? Datos { get; }
    public string Mensaje { get; }
    public IReadOnlyDictionary<string, string[]> Errores { get; }

    public static ResultadoApi<T> Correcto(T datos) => new(true, datos, string.Empty, SinErrores);

    public static ResultadoApi<T> Fallido(string mensaje, IReadOnlyDictionary<string, string[]>? errores = null) =>
        new(false, default, mensaje, errores ?? SinErrores);

    public string? ErrorDe(string campo)
    {
        var mensajes = Errores
            .Where(error => error.Key == campo || error.Key.StartsWith($"{campo}[", StringComparison.Ordinal))
            .SelectMany(error => error.Value)
            .ToList();

        return mensajes.Count == 0 ? null : string.Join(" ", mensajes);
    }
}
