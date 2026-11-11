using Microsoft.AspNetCore.Components;
using MPTextil.Web.Modelos;
using MPTextil.Web.Servicios;
using MudBlazor;

namespace MPTextil.Web.Componentes.Dialogos;

public abstract class DialogoAccionBase : ComponentBase
{
    [CascadingParameter] protected IMudDialogInstance? Dialogo { get; set; }

    [Inject] protected ApiSolicitudes ApiSolicitudes { get; set; } = default!;

    [Parameter] public int SolicitudId { get; set; }

    protected bool Procesando { get; private set; }

    protected string? MensajeLocal { get; set; }

    protected string? MensajeGeneral => MensajeLocal ?? (_resultado is { Exito: false, Errores.Count: 0 } resultado ? resultado.Mensaje : null);

    private ResultadoApi<RespuestaAccion>? _resultado;

    protected bool Tiene(string campo) => ErrorDe(campo) is not null;

    protected string? ErrorDe(string campo) => _resultado?.ErrorDe(campo);

    protected void Cancelar() => Dialogo?.Cancel();

    protected void LimpiarErrores()
    {
        _resultado = null;
        MensajeLocal = null;
    }

    protected async Task EnviarAsync(Func<Task<ResultadoApi<RespuestaAccion>>> accion)
    {
        Procesando = true;
        MensajeLocal = null;
        _resultado = await accion();
        Procesando = false;

        if (_resultado is { Exito: true, Datos: { } respuesta })
        {
            Dialogo?.Close(DialogResult.Ok(respuesta));
        }
    }
}
