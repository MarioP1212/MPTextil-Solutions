using FluentValidation;
using MPTextil.Application.Solicitudes.Acciones;
using MPTextil.Domain.Entidades;

namespace MPTextil.Api.Formularios;

public sealed class ValidadorFormularioEvaluacion : AbstractValidator<FormularioEvaluacion>
{
    private const long BytesPorMegabyte = 1024 * 1024;

    public ValidadorFormularioEvaluacion()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(formulario => formulario.Resultado)
            .IsInEnum().WithMessage("Selecciona el resultado de la evaluación.");

        RuleFor(formulario => formulario.Comentarios)
            .NotEmpty().WithMessage("Escribe tus comentarios sobre la muestra.")
            .MaximumLength(Evaluacion.ComentariosMaximo)
            .WithMessage($"Los comentarios no pueden superar {Evaluacion.ComentariosMaximo} caracteres.");

        RuleFor(formulario => formulario.Fotos)
            .Must(fotos => fotos.Count is >= Evaluacion.MinimoFotos and <= Evaluacion.MaximoFotos)
            .WithMessage($"Adjunta entre {Evaluacion.MinimoFotos} y {Evaluacion.MaximoFotos} fotos de la muestra.");

        RuleForEach(formulario => formulario.Fotos)
            .Must(foto => FotoEvaluacion.EsTipoPermitido(foto.ContentType))
            .WithMessage((_, foto) => $"La foto {foto.FileName} debe ser JPG, PNG o WEBP.")
            .Must(foto => foto.Length is > 0 and <= FotoEvaluacion.TamanoMaximoBytes)
            .WithMessage((_, foto) => $"La foto {foto.FileName} debe pesar como máximo {FotoEvaluacion.TamanoMaximoBytes / BytesPorMegabyte} MB.")
            .Must(TenerContenidoDeImagen)
            .WithMessage((_, foto) => $"El archivo {foto.FileName} no es una imagen válida.");
    }

    private static bool TenerContenidoDeImagen(IFormFile foto)
    {
        using var lectura = foto.OpenReadStream();
        Span<byte> cabecera = stackalloc byte[FirmaImagen.BytesNecesarios];
        var leidos = lectura.ReadAtLeast(cabecera, cabecera.Length, throwOnEndOfStream: false);

        return FirmaImagen.Coincide(cabecera[..leidos], foto.ContentType);
    }
}
