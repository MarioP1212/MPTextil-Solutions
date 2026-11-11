using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Domain.Flujo;

namespace MPTextil.Tests.Aplicacion.Soporte;

internal sealed class UsuarioActualFalso(Actor actor) : IUsuarioActual
{
    public Actor Actor { get; } = actor;
}
