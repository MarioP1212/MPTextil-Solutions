using MPTextil.Application.Solicitudes.Acciones;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Api.Formularios;

public sealed class FormularioEvaluacion
{
    public ResultadoEvaluacion Resultado { get; set; }
    public string Comentarios { get; set; } = string.Empty;
    public List<IFormFile> Fotos { get; set; } = [];

    public RegistrarEvaluacion ARegistrarEvaluacion() => new(
        Resultado,
        Comentarios,
        Fotos.Select(foto => new ArchivoFoto(
                Path.GetFileName(foto.FileName),
                foto.ContentType,
                foto.Length,
                foto.OpenReadStream))
            .ToList());
}
