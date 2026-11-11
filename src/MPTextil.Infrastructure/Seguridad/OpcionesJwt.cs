using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MPTextil.Infrastructure.Seguridad;

public sealed class OpcionesJwt
{
    public const string Seccion = "Jwt";
    public const int LongitudMinimaClave = 32;

    public string Emisor { get; init; } = string.Empty;
    public string Audiencia { get; init; } = string.Empty;
    public string Clave { get; init; } = string.Empty;
    public int MinutosExpiracion { get; init; } = 480;

    public bool EsValida =>
        !string.IsNullOrWhiteSpace(Emisor)
        && !string.IsNullOrWhiteSpace(Audiencia)
        && Encoding.UTF8.GetByteCount(Clave) >= LongitudMinimaClave
        && MinutosExpiracion > 0;

    public SymmetricSecurityKey CrearClaveFirma() => new(Encoding.UTF8.GetBytes(Clave));
}
