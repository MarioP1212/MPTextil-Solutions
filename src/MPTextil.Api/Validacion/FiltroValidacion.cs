using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;
using MPTextil.Application.Excepciones;

namespace MPTextil.Api.Validacion;

internal sealed class FiltroValidacion : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext contexto, ActionExecutionDelegate siguiente)
    {
        foreach (var argumento in contexto.ActionArguments.Values.OfType<object>())
        {
            var tipoValidador = typeof(IValidator<>).MakeGenericType(argumento.GetType());

            if (contexto.HttpContext.RequestServices.GetService(tipoValidador) is not IValidator validador)
            {
                continue;
            }

            var resultado = await validador.ValidateAsync(
                new ValidationContext<object>(argumento),
                contexto.HttpContext.RequestAborted);

            if (!resultado.IsValid)
            {
                throw new ValidacionException(resultado.ToDictionary());
            }
        }

        await siguiente();
    }
}
