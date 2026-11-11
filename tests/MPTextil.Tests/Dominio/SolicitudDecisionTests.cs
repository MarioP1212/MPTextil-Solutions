using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Excepciones;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Dominio;

public sealed class SolicitudDecisionTests
{
    [Fact]
    public void Aprobar_PorElSolicitanteCreador_QuedaPendienteDeAutorizacion()
    {
        var solicitud = Escenarios.EnEvaluacion();

        solicitud.Aprobar(Actores.Solicitante, DatosPrueba.EnDia());

        var decision = Assert.Single(solicitud.UltimaEvaluacion().Decisiones);
        Assert.Equal(EstadoSolicitud.PendienteAutorizacion, solicitud.Estado);
        Assert.Equal(NivelDecision.Aprobacion, decision.Nivel);
        Assert.True(decision.Aprobada);
        Assert.Equal(Actores.Solicitante.UsuarioId, decision.UsuarioId);
    }

    [Fact]
    public void Aprobar_PorOtroSolicitante_LanzaAccesoDenegado()
    {
        var solicitud = Escenarios.EnEvaluacion();

        var excepcion = Assert.Throws<AccesoDenegadoException>(() => solicitud.Aprobar(Actores.OtroSolicitante, DatosPrueba.EnDia()));

        Assert.Equal("Solo el solicitante que creó la solicitud puede realizar esta acción.", excepcion.Message);
        Assert.Equal(EstadoSolicitud.EnEvaluacion, solicitud.Estado);
    }

    [Fact]
    public void Autorizar_DespuesDeAprobar_CierraLaSolicitudConDosDecisiones()
    {
        var solicitud = Escenarios.PendienteDeAutorizacion();

        solicitud.Autorizar(Actores.Autorizador, DatosPrueba.EnDia());

        var decisiones = solicitud.UltimaEvaluacion().Decisiones;
        Assert.Equal(EstadoSolicitud.Aprobado, solicitud.Estado);
        Assert.Equal(2, decisiones.Count);
        Assert.All(decisiones, decision => Assert.True(decision.Aprobada));
        Assert.Contains(decisiones, decision => decision.Nivel == NivelDecision.Autorizacion);
    }

    [Fact]
    public void Autorizar_AntesDeLaAprobacion_LanzaTransicionInvalida()
    {
        var solicitud = Escenarios.EnEvaluacion();

        var excepcion = Assert.Throws<TransicionInvalidaException>(() => solicitud.Autorizar(Actores.Autorizador, DatosPrueba.EnDia()));

        Assert.Equal("No es posible autorizar la muestra en estado En evaluación.", excepcion.Message);
    }

    [Fact]
    public void Autorizar_PorElSolicitante_LanzaAccesoDenegado()
    {
        var solicitud = Escenarios.PendienteDeAutorizacion();

        Assert.Throws<AccesoDenegadoException>(() => solicitud.Autorizar(Actores.Solicitante, DatosPrueba.EnDia()));
    }

    [Fact]
    public void Rechazar_EnEvaluacion_RegistraRechazoDeAprobacionYRegresaAEnProgreso()
    {
        var solicitud = Escenarios.EnEvaluacion();

        solicitud.Rechazar("Color fuera de tono", Actores.Solicitante, DatosPrueba.EnDia());

        var decision = Assert.Single(solicitud.UltimaEvaluacion().Decisiones);
        Assert.Equal(EstadoSolicitud.EnProgreso, solicitud.Estado);
        Assert.Equal(NivelDecision.Aprobacion, decision.Nivel);
        Assert.False(decision.Aprobada);
        Assert.Equal("Color fuera de tono", decision.Motivo);
    }

    [Fact]
    public void Rechazar_PendienteDeAutorizacion_RegistraRechazoDeAutorizacion()
    {
        var solicitud = Escenarios.PendienteDeAutorizacion();

        solicitud.Rechazar("Falta etiqueta de composición", Actores.Autorizador, DatosPrueba.EnDia());

        var rechazo = solicitud.UltimaEvaluacion().Decisiones.Single(decision => !decision.Aprobada);
        Assert.Equal(EstadoSolicitud.EnProgreso, solicitud.Estado);
        Assert.Equal(NivelDecision.Autorizacion, rechazo.Nivel);
        Assert.Equal(Actores.Autorizador.UsuarioId, rechazo.UsuarioId);
    }

    [Fact]
    public void Rechazar_SinMotivo_LanzaReglaDeNegocioSinCambiarElEstado()
    {
        var solicitud = Escenarios.EnEvaluacion();

        var excepcion = Assert.Throws<ReglaNegocioException>(() => solicitud.Rechazar("  ", Actores.Solicitante, DatosPrueba.EnDia()));

        Assert.Equal("El campo motivo del rechazo es obligatorio.", excepcion.Message);
        Assert.Equal(EstadoSolicitud.EnEvaluacion, solicitud.Estado);
        Assert.Empty(solicitud.UltimaEvaluacion().Decisiones);
    }

    [Fact]
    public void Rechazar_PorElSolicitanteEnAutorizacion_LanzaTransicionInvalida()
    {
        var solicitud = Escenarios.PendienteDeAutorizacion();

        Assert.Throws<TransicionInvalidaException>(() => solicitud.Rechazar("Cambio de opinión", Actores.Solicitante, DatosPrueba.EnDia()));
    }

    [Fact]
    public void Rechazar_PorElCoordinador_LanzaAccesoDenegado()
    {
        var solicitud = Escenarios.EnEvaluacion();

        Assert.Throws<AccesoDenegadoException>(() => solicitud.Rechazar("No cumple", Actores.Coordinador, DatosPrueba.EnDia()));
    }

    [Fact]
    public void Rechazar_SolicitudAprobada_LanzaTransicionInvalida()
    {
        var solicitud = Escenarios.Aprobada();

        Assert.Throws<TransicionInvalidaException>(() => solicitud.Rechazar("Tarde", Actores.Autorizador, DatosPrueba.EnDia()));
    }
}
