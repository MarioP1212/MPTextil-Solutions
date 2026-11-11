using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Domain.Entidades;

namespace MPTextil.Tests.Aplicacion.Soporte;

internal sealed class GeneradorTokenFalso : IGeneradorToken
{
    public static readonly DateTime Expiracion = new(2026, 11, 11, 3, 0, 0, DateTimeKind.Utc);

    public TokenAcceso Generar(Usuario usuario) => new($"token-de-{usuario.Id}", Expiracion);
}
