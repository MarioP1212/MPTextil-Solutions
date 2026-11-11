namespace MPTextil.Application.Excepciones;

public sealed class RegistroDuplicadoException(Exception excepcionInterna)
    : Exception("Ya existe un registro con esos mismos datos. Revisa la información e intenta de nuevo.", excepcionInterna);
