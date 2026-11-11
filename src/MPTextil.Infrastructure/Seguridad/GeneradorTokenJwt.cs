using System.Globalization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Domain.Entidades;

namespace MPTextil.Infrastructure.Seguridad;

internal sealed class GeneradorTokenJwt(IOptions<OpcionesJwt> opciones, TimeProvider proveedorTiempo) : IGeneradorToken
{
    private readonly JsonWebTokenHandler _manejador = new();

    public TokenAcceso Generar(Usuario usuario)
    {
        var configuracion = opciones.Value;
        var emitido = proveedorTiempo.GetUtcNow().UtcDateTime;
        var expira = emitido.AddMinutes(configuracion.MinutosExpiracion);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = configuracion.Emisor,
            Audience = configuracion.Audiencia,
            IssuedAt = emitido,
            NotBefore = emitido,
            Expires = expira,
            Claims = CrearReclamos(usuario),
            SigningCredentials = new SigningCredentials(configuracion.CrearClaveFirma(), SecurityAlgorithms.HmacSha256)
        };

        return new TokenAcceso(_manejador.CreateToken(descriptor), expira);
    }

    private static Dictionary<string, object> CrearReclamos(Usuario usuario)
    {
        var reclamos = new Dictionary<string, object>
        {
            [ReclamosUsuario.Id] = usuario.Id.ToString(CultureInfo.InvariantCulture),
            [ReclamosUsuario.Nombre] = usuario.Nombre,
            [ReclamosUsuario.Correo] = usuario.Correo,
            [ReclamosUsuario.Rol] = usuario.Rol.ToString(),
            [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString()
        };

        if (usuario.ProveedorId is { } proveedorId)
        {
            reclamos[ReclamosUsuario.ProveedorId] = proveedorId.ToString(CultureInfo.InvariantCulture);
        }

        return reclamos;
    }
}
