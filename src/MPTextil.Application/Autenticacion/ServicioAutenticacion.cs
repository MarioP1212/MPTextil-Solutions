using MPTextil.Application.Abstracciones.Persistencia;
using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Application.Excepciones;

namespace MPTextil.Application.Autenticacion;

public sealed class ServicioAutenticacion(
    IUsuarioRepositorio usuarios,
    IHasherContrasena hasher,
    IGeneradorToken generadorToken)
{
    public async Task<RespuestaInicioSesion> IniciarSesionAsync(SolicitudInicioSesion solicitud, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.ObtenerPorCorreoAsync(solicitud.Correo, cancellationToken);

        if (usuario is null)
        {
            IgualarTiempoDeRespuesta(solicitud.Contrasena);
            throw new CredencialesInvalidasException();
        }

        var contrasenaCorrecta = hasher.Verificar(solicitud.Contrasena, usuario.PasswordHash);

        if (!usuario.Activo || !contrasenaCorrecta)
        {
            throw new CredencialesInvalidasException();
        }

        var token = generadorToken.Generar(usuario);

        return new RespuestaInicioSesion(token.Valor, token.ExpiraUtc, PerfilUsuario.Desde(usuario));
    }

    public async Task<PerfilUsuario> ObtenerPerfilAsync(int usuarioId, CancellationToken cancellationToken)
    {
        var usuario = await usuarios.ObtenerPorIdAsync(usuarioId, cancellationToken)
            ?? throw new RecursoNoEncontradoException("Tu usuario ya no existe. Contacta al administrador.");

        return PerfilUsuario.Desde(usuario);
    }

    private void IgualarTiempoDeRespuesta(string contrasena) => _ = hasher.Hashear(contrasena);
}
