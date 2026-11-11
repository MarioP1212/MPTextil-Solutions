using System.Text.Json;
using System.Text.Json.Serialization;

namespace MPTextil.Web.Servicios;

public static class OpcionesJson
{
    public static readonly JsonSerializerOptions Api = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}
