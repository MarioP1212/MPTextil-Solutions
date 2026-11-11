namespace MPTextil.Application.Abstracciones.Persistencia;

public interface IUnidadDeTrabajo
{
    Task GuardarCambiosAsync(CancellationToken cancellationToken = default);
}
