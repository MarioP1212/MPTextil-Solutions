namespace MPTextil.Application.Excepciones;

public sealed class ConflictoConcurrenciaException(Exception excepcionInterna)
    : Exception("Otro usuario modificó esta información mientras trabajabas en ella. Recarga la página e intenta de nuevo.", excepcionInterna);
