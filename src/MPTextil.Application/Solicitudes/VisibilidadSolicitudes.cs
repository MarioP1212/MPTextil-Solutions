using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Flujo;

namespace MPTextil.Application.Solicitudes;

internal static class VisibilidadSolicitudes
{
    public static int? ProveedorVisible(Actor actor) => actor.Rol == Rol.Proveedor
        ? actor.ProveedorId ?? throw new InvalidOperationException("El usuario proveedor no tiene una empresa asociada.")
        : null;

    public static bool PuedeVer(Actor actor, Solicitud solicitud) =>
        ProveedorVisible(actor) is not { } proveedorId || solicitud.ProveedorId == proveedorId;
}
