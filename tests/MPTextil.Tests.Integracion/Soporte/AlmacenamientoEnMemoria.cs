using System.Collections.Concurrent;
using MPTextil.Application.Abstracciones.Almacenamiento;

namespace MPTextil.Tests.Integracion.Soporte;

public sealed class AlmacenamientoEnMemoria : IAlmacenamientoArchivos
{
    private readonly ConcurrentDictionary<string, byte[]> _archivos = new();

    public IReadOnlyCollection<string> Nombres => _archivos.Keys.ToList();

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
        _archivos.TryRemove(nombre, out _);
        return Task.CompletedTask;
    }

    public string ObtenerUrlLectura(string nombre) => $"https://almacenamiento.prueba/{nombre}?firma=lectura";
}
