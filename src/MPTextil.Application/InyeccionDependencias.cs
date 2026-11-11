using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using MPTextil.Application.Autenticacion;
using MPTextil.Application.Catalogos;
using MPTextil.Application.Indicadores;
using MPTextil.Application.Solicitudes.Acciones;
using MPTextil.Application.Solicitudes.Consultas;

namespace MPTextil.Application;

public static class InyeccionDependencias
{
    public static IServiceCollection AgregarAplicacion(this IServiceCollection servicios)
    {
        servicios.AddValidatorsFromAssembly(typeof(InyeccionDependencias).Assembly);
        servicios.AddScoped<ServicioAutenticacion>();
        servicios.AddScoped<ServicioSolicitudes>();
        servicios.AddScoped<ServicioConsultaSolicitudes>();
        servicios.AddScoped<ServicioCatalogos>();
        servicios.AddScoped<ServicioIndicadores>();

        return servicios;
    }
}
