using Microsoft.OpenApi;

namespace MPTextil.Api.Documentacion;

internal static class ConfiguracionSwagger
{
    private const string Titulo = "MPTextil Solutions API";
    private const string EsquemaSeguridad = "Bearer";

    public static IServiceCollection AgregarDocumentacionApi(this IServiceCollection servicios)
    {
        servicios.AddEndpointsApiExplorer();
        servicios.AddSwaggerGen(opciones =>
        {
            opciones.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = Titulo,
                Version = "v1",
                Description = "Seguimiento de solicitudes de desarrollo de materiales: telas, mallas, etiquetas e hilos."
            });

            opciones.AddSecurityDefinition(EsquemaSeguridad, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Pega el token que devuelve POST /api/autenticacion/iniciar-sesion."
            });

            opciones.AddSecurityRequirement(documento => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(EsquemaSeguridad, documento)] = []
            });
        });

        return servicios;
    }

    public static WebApplication UsarDocumentacionApi(this WebApplication aplicacion)
    {
        aplicacion.UseSwagger();
        aplicacion.UseSwaggerUI(opciones =>
        {
            opciones.DocumentTitle = Titulo;
            opciones.EnablePersistAuthorization();
        });

        return aplicacion;
    }
}
