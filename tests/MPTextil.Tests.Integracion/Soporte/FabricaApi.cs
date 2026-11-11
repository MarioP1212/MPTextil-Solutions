using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MPTextil.Application.Abstracciones.Almacenamiento;

namespace MPTextil.Tests.Integracion.Soporte;

public sealed class FabricaApi(string cadenaConexion, IAlmacenamientoArchivos almacenamiento) : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment(Environments.Development);
        builder.UseSetting("ConnectionStrings:MPTextil", cadenaConexion);
        builder.ConfigureLogging(registro => registro.ClearProviders());
        builder.ConfigureTestServices(servicios =>
        {
            servicios.RemoveAll<IAlmacenamientoArchivos>();
            servicios.AddSingleton(almacenamiento);
        });
    }
}
