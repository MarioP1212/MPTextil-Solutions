namespace MPTextil.Infrastructure.Persistencia.Catalogos;

internal sealed class Catalogo<TEnum> where TEnum : struct, Enum
{
    public TEnum Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
}
