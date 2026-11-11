using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Flujo;

namespace MPTextil.Tests.Dominio.Soporte;

internal static class Escenarios
{
    public static Solicitud Crear(
        string nombreMaterial = "Jersey 180 g/m²",
        string descripcion = "Tela jersey 100% algodón para camisetas",
        TipoMaterial? tipoMaterial = null,
        Prioridad prioridad = Prioridad.Alta,
        DateOnly? fechaObjetivo = null,
        Actor? actor = null) =>
        Solicitud.Crear(
            nombreMaterial,
            descripcion,
            tipoMaterial ?? DatosPrueba.Tela(),
            prioridad,
            fechaObjetivo ?? DatosPrueba.Hoy.AddDays(30),
            actor ?? Actores.Solicitante,
            DatosPrueba.EnDia());

    public static Solicitud Solicitada() => Crear();

    public static Solicitud Asignada()
    {
        var solicitud = Solicitada();
        solicitud.Asignar(DatosPrueba.ProveedorAsignado(), Actores.Coordinador, DatosPrueba.EnDia());
        return solicitud;
    }

    public static Solicitud EnProgreso(int diasCompromiso = DatosPrueba.DiasCompromiso)
    {
        var solicitud = Asignada();
        solicitud.Aceptar(DatosPrueba.Hoy.AddDays(diasCompromiso), Actores.Proveedor, DatosPrueba.EnDia());
        return solicitud;
    }

    public static Solicitud Enviada()
    {
        var solicitud = EnProgreso();
        solicitud.RegistrarEnvio(DatosPrueba.Hoy, "DHL", "GUIA-001", Actores.Proveedor, DatosPrueba.EnDia());
        return solicitud;
    }

    public static Solicitud EnEvaluacion()
    {
        var solicitud = Enviada();
        solicitud.RegistrarEvaluacion(
            ResultadoEvaluacion.Cumple,
            "Muestra conforme a lo solicitado",
            DatosPrueba.Fotos(1),
            Actores.Coordinador,
            DatosPrueba.EnDia());
        return solicitud;
    }

    public static Solicitud PendienteDeAutorizacion()
    {
        var solicitud = EnEvaluacion();
        solicitud.Aprobar(Actores.Solicitante, DatosPrueba.EnDia());
        return solicitud;
    }

    public static Solicitud Aprobada()
    {
        var solicitud = PendienteDeAutorizacion();
        solicitud.Autorizar(Actores.Autorizador, DatosPrueba.EnDia());
        return solicitud;
    }

    public static Solicitud RechazadaEnDia(int dia)
    {
        var solicitud = EnEvaluacion();
        solicitud.Rechazar("Color fuera de tono", Actores.Solicitante, DatosPrueba.EnDia(dia));
        return solicitud;
    }
}
