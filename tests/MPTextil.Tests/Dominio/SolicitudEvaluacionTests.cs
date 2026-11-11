using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Excepciones;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Dominio;

public sealed class SolicitudEvaluacionTests
{
    [Fact]
    public void RegistrarEvaluacion_ConFotos_QuedaEnEvaluacion()
    {
        var solicitud = Escenarios.Enviada();

        solicitud.RegistrarEvaluacion(
            ResultadoEvaluacion.CumpleConObservaciones,
            "Tono levemente más claro",
            DatosPrueba.Fotos(3),
            Actores.Coordinador,
            DatosPrueba.EnDia());

        var evaluacion = solicitud.UltimaEvaluacion();
        Assert.Equal(EstadoSolicitud.EnEvaluacion, solicitud.Estado);
        Assert.Equal(ResultadoEvaluacion.CumpleConObservaciones, evaluacion.Resultado);
        Assert.Equal(Actores.Coordinador.UsuarioId, evaluacion.EvaluadoPorId);
        Assert.Equal(3, evaluacion.Fotos.Count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(Evaluacion.MaximoFotos + 1)]
    public void RegistrarEvaluacion_ConCantidadDeFotosFueraDeRango_LanzaReglaDeNegocio(int cantidad)
    {
        var solicitud = Escenarios.Enviada();

        var excepcion = Assert.Throws<ReglaNegocioException>(() => solicitud.RegistrarEvaluacion(
            ResultadoEvaluacion.Cumple,
            "Conforme",
            DatosPrueba.Fotos(cantidad),
            Actores.Coordinador,
            DatosPrueba.EnDia()));

        Assert.Equal($"Adjunta entre {Evaluacion.MinimoFotos} y {Evaluacion.MaximoFotos} fotos de la muestra.", excepcion.Message);
        Assert.Equal(EstadoSolicitud.Enviado, solicitud.Estado);
    }

    [Fact]
    public void RegistrarEvaluacion_SinComentarios_LanzaReglaDeNegocio()
    {
        var solicitud = Escenarios.Enviada();

        Assert.Throws<ReglaNegocioException>(() => solicitud.RegistrarEvaluacion(
            ResultadoEvaluacion.Cumple,
            string.Empty,
            DatosPrueba.Fotos(1),
            Actores.Coordinador,
            DatosPrueba.EnDia()));
    }

    [Fact]
    public void RegistrarEvaluacion_ConResultadoInexistente_LanzaReglaDeNegocio()
    {
        var solicitud = Escenarios.Enviada();

        Assert.Throws<ReglaNegocioException>(() => solicitud.RegistrarEvaluacion(
            (ResultadoEvaluacion)99,
            "Conforme",
            DatosPrueba.Fotos(1),
            Actores.Coordinador,
            DatosPrueba.EnDia()));
    }

    [Fact]
    public void RegistrarEvaluacion_PorProveedor_LanzaAccesoDenegado()
    {
        var solicitud = Escenarios.Enviada();

        Assert.Throws<AccesoDenegadoException>(() => solicitud.RegistrarEvaluacion(
            ResultadoEvaluacion.Cumple,
            "Conforme",
            DatosPrueba.Fotos(1),
            Actores.Proveedor,
            DatosPrueba.EnDia()));
    }

    [Fact]
    public void RegistrarEvaluacion_SolicitudSinEnviar_LanzaTransicionInvalida()
    {
        var solicitud = Escenarios.EnProgreso();

        Assert.Throws<TransicionInvalidaException>(() => solicitud.RegistrarEvaluacion(
            ResultadoEvaluacion.Cumple,
            "Conforme",
            DatosPrueba.Fotos(1),
            Actores.Coordinador,
            DatosPrueba.EnDia()));
    }
}
