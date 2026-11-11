using MPTextil.Application.Abstracciones.Persistencia;
using MPTextil.Domain.Entidades;

namespace MPTextil.Tests.Aplicacion.Soporte;

internal sealed class SolicitudRepositorioEnMemoria(params Solicitud[] solicitudes) : ISolicitudRepositorio
{
    private readonly List<Solicitud> _solicitudes = [.. solicitudes];

    public Task<Solicitud?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(_solicitudes.FirstOrDefault(solicitud => solicitud.Id == id));

    public void Agregar(Solicitud entidad) => _solicitudes.Add(entidad);
}

internal sealed class ProveedorRepositorioEnMemoria(params Proveedor[] proveedores) : IProveedorRepositorio
{
    public Task<Proveedor?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(proveedores.FirstOrDefault(proveedor => proveedor.Id == id));

    public Task<IReadOnlyList<Proveedor>> ListarActivosAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<Proveedor>>(proveedores.Where(proveedor => proveedor.Activo).ToList());

    public void Agregar(Proveedor entidad) => throw new NotSupportedException();
}

internal sealed class TipoMaterialRepositorioEnMemoria(params TipoMaterial[] tipos) : ITipoMaterialRepositorio
{
    public Task<TipoMaterial?> ObtenerPorIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(tipos.FirstOrDefault(tipo => tipo.Id == id));

    public Task<IReadOnlyList<TipoMaterial>> ListarActivosAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TipoMaterial>>(tipos.Where(tipo => tipo.Activo).ToList());

    public void Agregar(TipoMaterial entidad) => throw new NotSupportedException();
}

internal sealed class UnidadDeTrabajoFalsa(Exception? errorAlGuardar = null) : IUnidadDeTrabajo
{
    public int Guardados { get; private set; }

    public Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        if (errorAlGuardar is not null)
        {
            throw errorAlGuardar;
        }

        Guardados++;
        return Task.CompletedTask;
    }
}
