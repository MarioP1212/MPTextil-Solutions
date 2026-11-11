namespace MPTextil.Application.Abstracciones.Almacenamiento;

public interface IAlmacenamientoArchivos
{
    Task PrepararAsync(CancellationToken cancellationToken);

    Task GuardarAsync(string nombre, Stream contenido, string contentType, CancellationToken cancellationToken);

    Task<bool> ExisteAsync(string nombre, CancellationToken cancellationToken);

    Task EliminarSiExisteAsync(string nombre, CancellationToken cancellationToken);

    string ObtenerUrlLectura(string nombre);
}
