using System.Globalization;
using System.Security.Claims;
using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Flujo;

namespace MPTextil.Api.Seguridad;

internal sealed class UsuarioActualHttp(IHttpContextAccessor accesor) : IUsuarioActual
{
    public Actor Actor => CrearActor(accesor.HttpContext?.User);

    private static Actor CrearActor(ClaimsPrincipal? usuario)
    {
        if (usuario?.Identity?.IsAuthenticated is not true)
        {
            throw new InvalidOperationException("La petición actual no tiene un usuario autenticado.");
        }

        var usuarioId = LeerEntero(usuario, ReclamosUsuario.Id)
            ?? throw new InvalidOperationException("El token no contiene el identificador del usuario.");

        var rol = Enum.TryParse<Rol>(usuario.FindFirstValue(ReclamosUsuario.Rol), out var rolLeido)
            ? rolLeido
            : throw new InvalidOperationException("El token no contiene un rol válido.");

        return new Actor(usuarioId, rol, LeerEntero(usuario, ReclamosUsuario.ProveedorId));
    }

    private static int? LeerEntero(ClaimsPrincipal usuario, string tipoReclamo) =>
        int.TryParse(usuario.FindFirstValue(tipoReclamo), NumberStyles.Integer, CultureInfo.InvariantCulture, out var valor)
            ? valor
            : null;
}
