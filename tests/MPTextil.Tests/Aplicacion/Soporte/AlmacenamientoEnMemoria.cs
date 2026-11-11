using MPTextil.Application.Abstracciones.Almacenamiento;

namespace MPTextil.Tests.Aplicacion.Soporte;

internal sealed class AlmacenamientoEnMemoria : IAlmacenamientoArchivos
{
    private readonly Dictionary<string, byte[]> _archivos = [];

    public IReadOnlyCollection<string> Nombres => _archivos.Keys;
    public int Eliminados { get; private set; }

    public Task PrepararAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task GuardarAsync(string nombre, Stream contenido, string contentType, CancellationToken cancellationToken)
    {
        using var copia = new MemoryStream();
        await contenido.CopyToAsync(copia, cancellationToken);
        _archivos[nombre] = copia.ToArray();
    }

    public Task<bool> ExisteAsync(string nombre, CancellationToken cancellationToken) =>
        Task.FromResult(_archivos.ContainsKey(nombre));

    public Task EliminarSiExisteAsync(string nombre, CancellationToken cancellationToken)
    {
        if (_archivos.Remove(nombre))
        {
            Eliminados++;
        }

        return Task.CompletedTask;
    }

    public string ObtenerUrlLectura(string nombre) => $"https://almacenamiento.test/{nombre}?firma=lectura";
}
