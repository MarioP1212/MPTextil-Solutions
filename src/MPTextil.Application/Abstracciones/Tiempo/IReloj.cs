using MPTextil.Domain.Comun;

namespace MPTextil.Application.Abstracciones.Tiempo;

public interface IReloj
{
    Momento Ahora();
}
