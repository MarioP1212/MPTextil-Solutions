using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Excepciones;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Dominio;

public sealed class SolicitudHistorialTests
{
    [Fact]
    public void FlujoCompleto_RegistraCadaPasoEnOrden()
    {
        var solicitud = Escenarios.Aprobada();

        Assert.Equal(
            [
                AccionSolicitud.Crear,
                AccionSolicitud.Asignar,
                AccionSolicitud.Aceptar,
                AccionSolicitud.RegistrarEnvio,
                AccionSolicitud.RegistrarEvaluacion,
                AccionSolicitud.Aprobar,
                AccionSolicitud.Autorizar
            ],
            solicitud.Historial.Select(registro => registro.Accion));
    }

    [Fact]
    public void FlujoCompleto_RegistraQuienHizoCadaPaso()
    {
        var solicitud = Escenarios.Aprobada();

        Assert.Equal(
            [
                Actores.Solicitante.UsuarioId,
                Actores.Coordinador.UsuarioId,
                Actores.Proveedor.UsuarioId,
                Actores.Proveedor.UsuarioId,
                Actores.Coordinador.UsuarioId,
                Actores.Solicitante.UsuarioId,
                Actores.Autorizador.UsuarioId
            ],
            solicitud.Historial.Select(registro => registro.UsuarioId));
    }

    [Fact]
    public void FlujoCompleto_EncadenaLosEstados()
    {
        var registros = Escenarios.Aprobada().Historial.ToList();

        for (var indice = 1; indice < registros.Count; indice++)
        {
            Assert.Equal(registros[indice - 1].EstadoNuevo, registros[indice].EstadoAnterior);
        }
    }

    [Fact]
    public void AccionRechazada_NoAgregaRegistrosAlHistorial()
    {
        var solicitud = Escenarios.Asignada();
        var registrosAntes = solicitud.Historial.Count;

        Assert.Throws<AccesoDenegadoException>(() =>
            solicitud.Aceptar(DatosPrueba.Hoy.AddDays(5), Actores.OtroProveedor, DatosPrueba.EnDia()));

        Assert.Equal(registrosAntes, solicitud.Historial.Count);
    }

    [Fact]
    public void Rechazar_ConFechaPorVenir_DetallaMotivoYFechaConservada()
    {
        var solicitud = Escenarios.RechazadaEnDia(0);

        var detalle = solicitud.Historial.Last().Detalle;

        Assert.Equal(
            "Muestra rechazada en aprobación. Motivo: Color fuera de tono. Se mantiene la fecha compromiso 15/11/2026.",
            detalle);
    }

    [Fact]
    public void Rechazar_ConFechaVencida_DetallaQueSeRequiereNuevaFecha()
    {
        var solicitud = Escenarios.RechazadaEnDia(DatosPrueba.DiasCompromiso + 1);

        var detalle = solicitud.Historial.Last().Detalle;

        Assert.EndsWith("La fecha compromiso ya pasó, el proveedor debe registrar una nueva.", detalle);
    }
}
