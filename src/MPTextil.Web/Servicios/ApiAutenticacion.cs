using MPTextil.Web.Modelos;

namespace MPTextil.Web.Servicios;

public sealed class ApiAutenticacion(ClienteApi cliente, SesionActual sesion)
{
    public async Task<ResultadoApi<RespuestaInicioSesion>> IniciarSesionAsync(string correo, string contrasena)
    {
        var resultado = await cliente.PublicarAsync<RespuestaInicioSesion>(
            "api/autenticacion/iniciar-sesion",
            new SolicitudInicioSesion(correo, contrasena));

        if (resultado is { Exito: true, Datos: { } respuesta })
        {
            await sesion.IniciarAsync(respuesta);
        }

        return resultado;
    }

    public Task CerrarSesionAsync() => sesion.CerrarAsync();
}
