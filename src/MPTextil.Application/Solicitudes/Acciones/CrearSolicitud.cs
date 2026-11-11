using FluentValidation;
using MPTextil.Application.Abstracciones.Tiempo;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;

namespace MPTextil.Application.Solicitudes.Acciones;

public sealed record CrearSolicitud(
    string NombreMaterial,
    string Descripcion,
    int TipoMaterialId,
    Prioridad Prioridad,
    DateOnly FechaObjetivo);

public sealed class ValidadorCrearSolicitud : AbstractValidator<CrearSolicitud>
{
    public ValidadorCrearSolicitud(IReloj reloj)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(datos => datos.NombreMaterial)
            .NotEmpty().WithMessage("Escribe el nombre del material.")
            .MaximumLength(Solicitud.NombreMaterialMaximo)
            .WithMessage($"El nombre del material no puede superar {Solicitud.NombreMaterialMaximo} caracteres.");

        RuleFor(datos => datos.Descripcion)
            .NotEmpty().WithMessage("Describe el material que necesitas.")
            .MaximumLength(Solicitud.DescripcionMaxima)
            .WithMessage($"La descripción no puede superar {Solicitud.DescripcionMaxima} caracteres.");

        RuleFor(datos => datos.TipoMaterialId)
            .GreaterThan(0).WithMessage("Selecciona el tipo de material.");

        RuleFor(datos => datos.Prioridad)
            .IsInEnum().WithMessage("Selecciona una prioridad válida.");

        RuleFor(datos => datos.FechaObjetivo)
            .NotEmpty().WithMessage("Indica la fecha objetivo.")
            .GreaterThanOrEqualTo(_ => reloj.Ahora().Hoy).WithMessage("La fecha objetivo no puede ser anterior a hoy.");
    }
}
