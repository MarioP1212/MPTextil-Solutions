namespace MPTextil.Application.Excepciones;

public sealed class CredencialesInvalidasException()
    : Exception("El correo o la contraseña no son correctos.");
