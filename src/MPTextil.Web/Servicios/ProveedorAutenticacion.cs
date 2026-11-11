using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using MPTextil.Web.Modelos;

namespace MPTextil.Web.Servicios;

public sealed class ProveedorAutenticacion : AuthenticationStateProvider, IDisposable
{
    private const string TipoAutenticacion = "jwt";

    private readonly SesionActual _sesion;

    public ProveedorAutenticacion(SesionActual sesion)
    {
        _sesion = sesion;
        _sesion.Cambio += Notificar;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync() =>
        Task.FromResult(new AuthenticationState(CrearPrincipal(_sesion.Usuario)));

    public void Dispose() => _sesion.Cambio -= Notificar;

    private void Notificar() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    private static ClaimsPrincipal CrearPrincipal(PerfilUsuario? usuario)
    {
        if (usuario is null)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }

        Claim[] reclamos =
        [
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Email, usuario.Correo),
            new(ClaimTypes.Role, usuario.Rol.ToString())
        ];

        return new ClaimsPrincipal(new ClaimsIdentity(reclamos, TipoAutenticacion));
    }
}
