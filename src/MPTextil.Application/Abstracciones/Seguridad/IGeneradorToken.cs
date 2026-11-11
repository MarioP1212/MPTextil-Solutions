using MPTextil.Domain.Entidades;

namespace MPTextil.Application.Abstracciones.Seguridad;

public interface IGeneradorToken
{
    TokenAcceso Generar(Usuario usuario);
}

public sealed record TokenAcceso(string Valor, DateTime ExpiraUtc);
