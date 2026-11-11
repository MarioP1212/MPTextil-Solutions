namespace MPTextil.Application.Excepciones;

public sealed class RecursoNoEncontradoException(string mensaje) : Exception(mensaje);
