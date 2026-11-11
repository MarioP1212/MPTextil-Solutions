using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Api.Seguridad;

internal static class Politicas
{
    public const string Solicitante = nameof(Rol.Solicitante);
    public const string Coordinador = nameof(Rol.Coordinador);
    public const string Proveedor = nameof(Rol.Proveedor);
    public const string Autorizador = nameof(Rol.Autorizador);
    public const string DecisorMuestra = "DecisorMuestra";
}
