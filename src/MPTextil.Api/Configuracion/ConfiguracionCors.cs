namespace MPTextil.Api.Configuracion;

internal static class ConfiguracionCors
{
    private const string Politica = "FrontMPTextil";
    private const string ClaveOrigenes = "Cors:OrigenesPermitidos";

    public static IServiceCollection AgregarCors(this IServiceCollection servicios, IConfiguration configuracion)
    {
        var origenes = configuracion.GetSection(ClaveOrigenes).Get<string[]>() ?? [];

        servicios.AddCors(opciones => opciones.AddPolicy(Politica, politica => politica
            .WithOrigins(origenes)
            .AllowAnyHeader()
            .AllowAnyMethod()));

        return servicios;
    }

    public static WebApplication UsarCors(this WebApplication aplicacion)
    {
        aplicacion.UseCors(Politica);
        return aplicacion;
    }
}
