using MPTextil.Application.Abstracciones.Persistencia;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Application.Catalogos;

public sealed class ServicioCatalogos(IProveedorRepositorio proveedores, ITipoMaterialRepositorio tiposMaterial)
{
    public async Task<IReadOnlyList<ElementoCatalogo>> ListarProveedoresAsync(CancellationToken cancellationToken) =>
        (await proveedores.ListarActivosAsync(cancellationToken))
            .Select(proveedor => new ElementoCatalogo(proveedor.Id, proveedor.Nombre))
            .ToList();

    public async Task<IReadOnlyList<ElementoCatalogo>> ListarTiposMaterialAsync(CancellationToken cancellationToken) =>
        (await tiposMaterial.ListarActivosAsync(cancellationToken))
            .Select(tipo => new ElementoCatalogo(tipo.Id, tipo.Nombre))
            .ToList();

    public IReadOnlyList<OpcionCatalogo> ListarPrioridades() => OpcionesDe<Prioridad>(prioridad => prioridad.Descripcion());

    public IReadOnlyList<OpcionCatalogo> ListarEstados() => OpcionesDe<EstadoSolicitud>(estado => estado.Descripcion());

    private static List<OpcionCatalogo> OpcionesDe<TEnum>(Func<TEnum, string> describir) where TEnum : struct, Enum =>
        Enum.GetValues<TEnum>()
            .Select(valor => new OpcionCatalogo(valor.ToString(), describir(valor)))
            .ToList();
}
