namespace MPTextil.Domain.Excepciones;

public sealed class AccesoDenegadoException(string mensaje) : DominioException(mensaje);
