using MPTextil.Domain.Flujo;

namespace MPTextil.Application.Abstracciones.Seguridad;

public interface IUsuarioActual
{
    Actor Actor { get; }
}
