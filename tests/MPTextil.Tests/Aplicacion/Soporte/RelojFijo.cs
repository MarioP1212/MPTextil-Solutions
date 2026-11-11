using MPTextil.Application.Abstracciones.Tiempo;
using MPTextil.Domain.Comun;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Aplicacion.Soporte;

internal sealed class RelojFijo(int dia = 0) : IReloj
{
    public Momento Ahora() => DatosPrueba.EnDia(dia);
}
