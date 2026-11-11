using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Infrastructure.Seguridad;

namespace MPTextil.Api.Seguridad;

internal static class ConfiguracionSeguridad
{
    private static readonly TimeSpan ToleranciaReloj = TimeSpan.FromMinutes(1);

    public static IServiceCollection AgregarSeguridad(this IServiceCollection servicios)
    {
        servicios.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

        servicios.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<OpcionesJwt>>((portador, opcionesJwt) =>
            {
                var jwt = opcionesJwt.Value;
                portador.MapInboundClaims = false;
                portador.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Emisor,
                    ValidAudience = jwt.Audiencia,
                    IssuerSigningKey = jwt.CrearClaveFirma(),
                    NameClaimType = ReclamosUsuario.Nombre,
                    RoleClaimType = ReclamosUsuario.Rol,
                    ClockSkew = ToleranciaReloj
                };
            });

        var autorizacion = servicios.AddAuthorizationBuilder()
            .SetFallbackPolicy(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        foreach (var rol in Enum.GetValues<Rol>())
        {
            autorizacion.AddPolicy(rol.ToString(), politica => politica.RequireRole(rol.ToString()));
        }

        autorizacion.AddPolicy(
            Politicas.DecisorMuestra,
            politica => politica.RequireRole(Politicas.Solicitante, Politicas.Autorizador));

        servicios.AddHttpContextAccessor();
        servicios.AddScoped<IUsuarioActual, UsuarioActualHttp>();

        return servicios;
    }
}
