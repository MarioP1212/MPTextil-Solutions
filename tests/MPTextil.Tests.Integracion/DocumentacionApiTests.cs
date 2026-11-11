using System.Net;
using MPTextil.Tests.Integracion.Soporte;

namespace MPTextil.Tests.Integracion;

[Collection(ColeccionApi.Nombre)]
public sealed class DocumentacionApiTests(ApiDePrueba api)
{
    [Theory]
    [InlineData("/api/autenticacion/iniciar-sesion")]
    [InlineData("/api/solicitudes/{id}/registrar-evaluacion")]
    [InlineData("/api/indicadores")]
    public async Task Swagger_SinToken_DocumentaLosEndpoints(string ruta)
    {
        using var cliente = api.ClienteAnonimo();

        using var respuesta = await cliente.GetAsync("swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        Assert.Contains($"\"{ruta}\"", await respuesta.Content.ReadAsStringAsync());
    }
}
