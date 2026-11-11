namespace MPTextil.Application.Autenticacion;

public sealed record RespuestaInicioSesion(string Token, DateTime ExpiraUtc, PerfilUsuario Usuario);
