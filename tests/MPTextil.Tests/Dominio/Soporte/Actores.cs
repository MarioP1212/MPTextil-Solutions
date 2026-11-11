using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Flujo;

namespace MPTextil.Tests.Dominio.Soporte;

internal static class Actores
{
    public static readonly Actor Solicitante = new(1, Rol.Solicitante, null);
    public static readonly Actor OtroSolicitante = new(2, Rol.Solicitante, null);
    public static readonly Actor Coordinador = new(3, Rol.Coordinador, null);
    public static readonly Actor Autorizador = new(4, Rol.Autorizador, null);
    public static readonly Actor Proveedor = new(5, Rol.Proveedor, DatosPrueba.ProveedorAsignadoId);
    public static readonly Actor OtroProveedor = new(6, Rol.Proveedor, DatosPrueba.OtroProveedorId);
}
