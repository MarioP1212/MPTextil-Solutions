using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Excepciones;
using MPTextil.Domain.Flujo;
using MPTextil.Tests.Dominio.Soporte;

namespace MPTextil.Tests.Dominio;

public sealed class SolicitudCreacionTests
{
    [Fact]
    public void Crear_ConDatosValidos_QuedaSolicitadaSinProveedor()
    {
        var solicitud = Escenarios.Solicitada();

        Assert.Equal(EstadoSolicitud.Solicitado, solicitud.Estado);
        Assert.Equal(Actores.Solicitante.UsuarioId, solicitud.SolicitanteId);
        Assert.Equal(DatosPrueba.TipoMaterialId, solicitud.TipoMaterialId);
        Assert.Null(solicitud.ProveedorId);
        Assert.Empty(solicitud.Compromisos);
    }

    [Fact]
    public void Crear_ConDatosValidos_RegistraLaCreacionEnElHistorial()
    {
        var solicitud = Escenarios.Solicitada();

        var registro = Assert.Single(solicitud.Historial);
        Assert.Equal(AccionSolicitud.Crear, registro.Accion);
        Assert.Null(registro.EstadoAnterior);
        Assert.Equal(EstadoSolicitud.Solicitado, registro.EstadoNuevo);
        Assert.Equal(Actores.Solicitante.UsuarioId, registro.UsuarioId);
    }

    [Fact]
    public void Crear_ConEspaciosAlrededor_GuardaLosTextosRecortados()
    {
        var solicitud = Escenarios.Crear(nombreMaterial: "  Malla deportiva  ", descripcion: "  Malla transpirable  ");

        Assert.Equal("Malla deportiva", solicitud.NombreMaterial);
        Assert.Equal("Malla transpirable", solicitud.Descripcion);
    }

    [Fact]
    public void Crear_ConFechaObjetivoHoy_EsValida()
    {
        var solicitud = Escenarios.Crear(fechaObjetivo: DatosPrueba.Hoy);

        Assert.Equal(DatosPrueba.Hoy, solicitud.FechaObjetivo);
    }

    [Theory]
    [InlineData(Rol.Coordinador)]
    [InlineData(Rol.Proveedor)]
    [InlineData(Rol.Autorizador)]
    public void Crear_ConRolDistintoASolicitante_LanzaAccesoDenegado(Rol rol)
    {
        var actor = new Actor(99, rol, rol == Rol.Proveedor ? DatosPrueba.ProveedorAsignadoId : null);

        Assert.Throws<AccesoDenegadoException>(() => Escenarios.Crear(actor: actor));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Crear_SinNombreDeMaterial_LanzaReglaDeNegocio(string nombreMaterial)
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(() => Escenarios.Crear(nombreMaterial: nombreMaterial));

        Assert.Equal("El campo nombre del material es obligatorio.", excepcion.Message);
    }

    [Fact]
    public void Crear_ConNombreDeMaterialMuyLargo_LanzaReglaDeNegocio()
    {
        var nombreMaterial = new string('a', Solicitud.NombreMaterialMaximo + 1);

        var excepcion = Assert.Throws<ReglaNegocioException>(() => Escenarios.Crear(nombreMaterial: nombreMaterial));

        Assert.Equal($"El campo nombre del material no puede superar {Solicitud.NombreMaterialMaximo} caracteres.", excepcion.Message);
    }

    [Fact]
    public void Crear_SinDescripcion_LanzaReglaDeNegocio()
    {
        Assert.Throws<ReglaNegocioException>(() => Escenarios.Crear(descripcion: " "));
    }

    [Fact]
    public void Crear_ConFechaObjetivoPasada_LanzaReglaDeNegocio()
    {
        var excepcion = Assert.Throws<ReglaNegocioException>(() => Escenarios.Crear(fechaObjetivo: DatosPrueba.Hoy.AddDays(-1)));

        Assert.Equal("La fecha objetivo no puede ser anterior a hoy.", excepcion.Message);
    }

    [Fact]
    public void Crear_ConTipoDeMaterialInactivo_LanzaReglaDeNegocio()
    {
        var tipoMaterial = DatosPrueba.Tela();
        tipoMaterial.Desactivar();

        Assert.Throws<ReglaNegocioException>(() => Escenarios.Crear(tipoMaterial: tipoMaterial));
    }

    [Fact]
    public void Crear_ConPrioridadInexistente_LanzaReglaDeNegocio()
    {
        Assert.Throws<ReglaNegocioException>(() => Escenarios.Crear(prioridad: (Prioridad)99));
    }
}
