using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MPTextil.Application.Abstracciones.Persistencia;
using MPTextil.Application.Excepciones;

namespace MPTextil.Infrastructure.Persistencia;

internal sealed class UnidadDeTrabajo(MPTextilDbContext contexto) : IUnidadDeTrabajo
{
    private const int IndiceUnicoDuplicado = 2601;
    private const int RestriccionUnicaDuplicada = 2627;

    public async Task GuardarCambiosAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await contexto.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException excepcion)
        {
            throw new ConflictoConcurrenciaException(excepcion);
        }
        catch (DbUpdateException excepcion) when (excepcion.InnerException is SqlException { Number: IndiceUnicoDuplicado or RestriccionUnicaDuplicada } errorSql)
        {
            throw EsConflictoDeConcurrencia(errorSql)
                ? new ConflictoConcurrenciaException(excepcion)
                : new RegistroDuplicadoException(excepcion);
        }
    }

    private static bool EsConflictoDeConcurrencia(SqlException errorSql) =>
        IndicesConcurrencia.Todos.Any(indice => errorSql.Message.Contains(indice, StringComparison.Ordinal));
}
