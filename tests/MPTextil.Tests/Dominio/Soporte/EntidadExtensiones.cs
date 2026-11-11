using MPTextil.Domain.Comun;
using MPTextil.Domain.Entidades;

namespace MPTextil.Tests.Dominio.Soporte;

internal static class EntidadExtensiones
{
    public static T ConId<T>(this T entidad, int id) where T : Entidad
    {
        typeof(Entidad<int>).GetProperty(nameof(Entidad.Id))!.SetValue(entidad, id);
        return entidad;
    }

    public static Evaluacion UltimaEvaluacion(this Solicitud solicitud) =>
        solicitud.Compromisos
            .Select(compromiso => compromiso.Envio?.Evaluacion)
            .OfType<Evaluacion>()
            .Last();
}
