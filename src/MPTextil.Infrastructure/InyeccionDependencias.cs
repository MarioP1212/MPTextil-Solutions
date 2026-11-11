using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MPTextil.Application.Abstracciones.Almacenamiento;
using MPTextil.Application.Abstracciones.Consultas;
using MPTextil.Application.Abstracciones.Persistencia;
using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Application.Abstracciones.Tiempo;
using MPTextil.Infrastructure.Almacenamiento;
using MPTextil.Infrastructure.Consultas;
using MPTextil.Infrastructure.Persistencia;
using MPTextil.Infrastructure.Persistencia.DatosEjemplo;
using MPTextil.Infrastructure.Persistencia.Repositorios;
using MPTextil.Infrastructure.Seguridad;
using MPTextil.Infrastructure.Tiempo;

namespace MPTextil.Infrastructure;

public static class InyeccionDependencias
{
    private const string NombreCadenaConexion = "MPTextil";
    private const string ClaveZonaHoraria = "ZonaHoraria";

    public static IServiceCollection AgregarInfraestructura(this IServiceCollection servicios, IConfiguration configuracion)
    {
        var cadenaConexion = configuracion.GetConnectionString(NombreCadenaConexion)
            ?? throw new InvalidOperationException($"Falta la cadena de conexión '{NombreCadenaConexion}' en la configuración.");

        var zonaHoraria = TimeZoneInfo.FindSystemTimeZoneById(configuracion[ClaveZonaHoraria]
            ?? throw new InvalidOperationException($"Falta la clave '{ClaveZonaHoraria}' en la configuración."));

        servicios.AddDbContext<MPTextilDbContext>(opciones =>
            opciones.UseSqlServer(cadenaConexion, sql => sql.EnableRetryOnFailure()));

        servicios.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
        servicios.AddScoped<ISolicitudRepositorio, SolicitudRepositorio>();
        servicios.AddScoped<IUsuarioRepositorio, UsuarioRepositorio>();
        servicios.AddScoped<IProveedorRepositorio, ProveedorRepositorio>();
        servicios.AddScoped<ITipoMaterialRepositorio, TipoMaterialRepositorio>();
        servicios.AddScoped<IConsultasSolicitudes, ConsultasSolicitudes>();
        servicios.AddScoped<IConsultasIndicadores, ConsultasIndicadores>();
        servicios.AddScoped<SembradorDatosEjemplo>();
        servicios.AddScoped<SincronizadorFotosEjemplo>();

        servicios.AddOptions<OpcionesAlmacenamiento>()
            .Bind(configuracion.GetSection(OpcionesAlmacenamiento.Seccion))
            .Validate(
                opciones => opciones.EsValida,
                $"Revisa la sección '{OpcionesAlmacenamiento.Seccion}': la cadena de conexión y el contenedor son obligatorios.")
            .ValidateOnStart();
        servicios.AddSingleton(proveedor =>
        {
            var opciones = proveedor.GetRequiredService<IOptions<OpcionesAlmacenamiento>>().Value;
            return new BlobContainerClient(opciones.CadenaConexion, opciones.Contenedor);
        });
        servicios.AddSingleton<IAlmacenamientoArchivos, AlmacenamientoBlobAzure>();

        servicios.AddSingleton(TimeProvider.System);
        servicios.AddSingleton<IReloj>(proveedor => new RelojZonaHoraria(proveedor.GetRequiredService<TimeProvider>(), zonaHoraria));
        servicios.AddSingleton<IHasherContrasena, HasherContrasenaPbkdf2>();

        servicios.AddOptions<OpcionesJwt>()
            .Bind(configuracion.GetSection(OpcionesJwt.Seccion))
            .Validate(
                opciones => opciones.EsValida,
                $"Revisa la sección '{OpcionesJwt.Seccion}': el emisor, la audiencia y una clave de al menos {OpcionesJwt.LongitudMinimaClave} caracteres son obligatorios.")
            .ValidateOnStart();
        servicios.AddSingleton<IGeneradorToken, GeneradorTokenJwt>();

        return servicios;
    }
}
