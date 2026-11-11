using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MPTextil.Application.Abstracciones.Almacenamiento;

namespace MPTextil.Infrastructure.Almacenamiento;

internal sealed class AlmacenamientoBlobAzure(
    BlobContainerClient contenedor,
    IOptions<OpcionesAlmacenamiento> opciones,
    TimeProvider proveedorTiempo,
    ILogger<AlmacenamientoBlobAzure> registro) : IAlmacenamientoArchivos
{
    public async Task PrepararAsync(CancellationToken cancellationToken)
    {
        try
        {
            await contenedor.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken);
        }
        catch (Exception excepcion) when (excepcion is RequestFailedException or AggregateException)
        {
            throw new InvalidOperationException(
                $"No se pudo conectar al almacenamiento de archivos. En desarrollo verifica que Azurite esté corriendo; en la nube revisa '{OpcionesAlmacenamiento.Seccion}:{nameof(OpcionesAlmacenamiento.CadenaConexion)}'.",
                excepcion);
        }
    }

    public async Task GuardarAsync(string nombre, Stream contenido, string contentType, CancellationToken cancellationToken)
    {
        var opcionesSubida = new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentType = contentType } };

        await contenedor.GetBlobClient(nombre).UploadAsync(contenido, opcionesSubida, cancellationToken);
    }

    public async Task<bool> ExisteAsync(string nombre, CancellationToken cancellationToken) =>
        await contenedor.GetBlobClient(nombre).ExistsAsync(cancellationToken);

    public async Task EliminarSiExisteAsync(string nombre, CancellationToken cancellationToken)
    {
        try
        {
            await contenedor.GetBlobClient(nombre).DeleteIfExistsAsync(cancellationToken: cancellationToken);
        }
        catch (RequestFailedException excepcion)
        {
            registro.LogWarning(excepcion, "No se pudo eliminar el archivo {Nombre} del almacenamiento", nombre);
        }
    }

    public string ObtenerUrlLectura(string nombre)
    {
        var expira = proveedorTiempo.GetUtcNow().AddMinutes(opciones.Value.MinutosVigenciaEnlaces);

        return contenedor.GetBlobClient(nombre).GenerateSasUri(BlobSasPermissions.Read, expira).ToString();
    }
}
