namespace MPTextil.Domain.Excepciones;

public sealed class ReglaNegocioException(string mensaje) : DominioException(mensaje);
