using Microsoft.EntityFrameworkCore;
using MPTextil.Application.Abstracciones.Seguridad;
using MPTextil.Application.Abstracciones.Tiempo;
using MPTextil.Domain.Entidades;
using MPTextil.Domain.Enumeraciones;
using MPTextil.Domain.Flujo;

namespace MPTextil.Infrastructure.Persistencia.DatosEjemplo;

internal sealed class SembradorDatosEjemplo(MPTextilDbContext contexto, IHasherContrasena hasher, IReloj reloj)
{
    private const string BloqueoSiembra =
        "EXEC sp_getapplock @Resource = N'MPTextil.DatosEjemplo', @LockMode = N'Exclusive', @LockOwner = N'Transaction', @LockTimeout = 60000";

    public async Task SembrarAsync(string contrasena, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(contrasena))
        {
            throw new InvalidOperationException(
                $"Configura '{OpcionesDatosEjemplo.Seccion}:{nameof(OpcionesDatosEjemplo.Contrasena)}' para sembrar los datos de ejemplo.");
        }

        if (await contexto.Usuarios.AnyAsync(cancellationToken))
        {
            return;
        }

        var estrategia = contexto.Database.CreateExecutionStrategy();
        await estrategia.ExecuteAsync(async () =>
        {
            contexto.ChangeTracker.Clear();
            await using var transaccion = await contexto.Database.BeginTransactionAsync(cancellationToken);
            await contexto.Database.ExecuteSqlRawAsync(BloqueoSiembra, cancellationToken);

            if (await contexto.Usuarios.AnyAsync(cancellationToken))
            {
                return;
            }

            var ahora = reloj.Ahora();
            var proveedores = await CrearProveedoresAsync(cancellationToken);
            var usuarios = await CrearUsuariosAsync(proveedores, contrasena, ahora.Utc, cancellationToken);
            var tipos = await contexto.TiposMaterial.ToDictionaryAsync(tipo => tipo.Nombre, cancellationToken);

            var participantes = new ParticipantesEjemplo(
                ahora,
                usuarios.Coordinador.ComoActor(),
                usuarios.Autorizador.ComoActor(),
                usuarios.PorProveedor.ToDictionary(par => par.Key, par => par.Value.ComoActor()));

            contexto.Solicitudes.AddRange(CrearSolicitudes(participantes, usuarios, proveedores, tipos));
            await contexto.SaveChangesAsync(cancellationToken);

            await transaccion.CommitAsync(cancellationToken);
        });
    }

    private async Task<ProveedoresEjemplo> CrearProveedoresAsync(CancellationToken cancellationToken)
    {
        var proveedores = new ProveedoresEjemplo(
            Proveedor.Crear("Textiles del Norte", "ventas@textilesdelnorte.example", "2550-1010"),
            Proveedor.Crear("Hilos y Fibras del Valle", "contacto@hilosdelvalle.example", "2556-2020"),
            Proveedor.Crear("Etiquetas Centroamericanas", "pedidos@etiquetasca.example", "2552-3030"),
            Proveedor.Crear("Mallas Técnicas del Caribe", "ventas@mallascaribe.example", "2443-4040"));

        contexto.Proveedores.AddRange(proveedores.Todos);
        await contexto.SaveChangesAsync(cancellationToken);

        return proveedores;
    }

    private async Task<UsuariosEjemplo> CrearUsuariosAsync(
        ProveedoresEjemplo proveedores,
        string contrasena,
        DateTime fechaUtc,
        CancellationToken cancellationToken)
    {
        Usuario Interno(string nombre, string correo, Rol rol) =>
            Usuario.Crear(nombre, correo, hasher.Hashear(contrasena), rol, null, fechaUtc);

        Usuario DeProveedor(string nombre, string correo, Proveedor proveedor) =>
            Usuario.Crear(nombre, correo, hasher.Hashear(contrasena), Rol.Proveedor, proveedor.Id, fechaUtc);

        var usuarios = new UsuariosEjemplo(
            Interno("Sofía Martínez", "sofia.martinez@mptextil.example", Rol.Solicitante),
            Interno("Diego Ramírez", "diego.ramirez@mptextil.example", Rol.Solicitante),
            Interno("Carlos Mejía", "carlos.mejia@mptextil.example", Rol.Coordinador),
            Interno("Ana Castillo", "ana.castillo@mptextil.example", Rol.Autorizador),
            new Dictionary<int, Usuario>
            {
                [proveedores.TextilesDelNorte.Id] = DeProveedor("Pedro López", "pedro.lopez@textilesdelnorte.example", proveedores.TextilesDelNorte),
                [proveedores.HilosDelValle.Id] = DeProveedor("Lucía Reyes", "lucia.reyes@hilosdelvalle.example", proveedores.HilosDelValle),
                [proveedores.EtiquetasCentroamericanas.Id] = DeProveedor("Jorge Flores", "jorge.flores@etiquetasca.example", proveedores.EtiquetasCentroamericanas),
                [proveedores.MallasDelCaribe.Id] = DeProveedor("Marta Gómez", "marta.gomez@mallascaribe.example", proveedores.MallasDelCaribe)
            });

        contexto.Usuarios.AddRange(usuarios.Todos);
        await contexto.SaveChangesAsync(cancellationToken);

        return usuarios;
    }

    private static IEnumerable<Solicitud> CrearSolicitudes(
        ParticipantesEjemplo participantes,
        UsuariosEjemplo usuarios,
        ProveedoresEjemplo proveedores,
        IReadOnlyDictionary<string, TipoMaterial> tipos)
    {
        var sofia = usuarios.SolicitanteSofia.ComoActor();
        var diego = usuarios.SolicitanteDiego.ComoActor();
        var tela = tipos["Tela"];
        var malla = tipos["Malla"];
        var etiqueta = tipos["Etiqueta"];
        var hilo = tipos["Hilo"];

        CronologiaSolicitud Nueva(
            Actor solicitante, string material, string descripcion, TipoMaterial tipo, Prioridad prioridad, int plazoDias, int dia) =>
            CronologiaSolicitud.Crear(participantes, solicitante, material, descripcion, tipo, prioridad, plazoDias, dia);

        return
        [
            Nueva(sofia, "Jersey piqué 220 g/m²", "Tela piqué 100% algodón para polos de la línea ejecutiva, color azul marino.", tela, Prioridad.Alta, 25, -1)
                .Solicitud,

            Nueva(diego, "Etiqueta tejida de marca", "Etiqueta tejida en damasco con logotipo bordado para cuello, 5 x 2 cm.", etiqueta, Prioridad.Media, 30, 0)
                .Solicitud,

            Nueva(sofia, "Hilo poliéster 40/2 negro", "Hilo de costura poliéster 40/2 para pespunte en pantalones de mezclilla.", hilo, Prioridad.Media, 20, -3)
                .Asignar(proveedores.HilosDelValle, -2)
                .Solicitud,

            Nueva(diego, "Malla mesh deportiva", "Malla mesh transpirable para paneles laterales de camisetas deportivas.", malla, Prioridad.Alta, 25, -6)
                .Asignar(proveedores.TextilesDelNorte, -5)
                .Declinar("No contamos con maquinaria para tejido de malla en este momento.", -4)
                .Asignar(proveedores.MallasDelCaribe, -3)
                .Solicitud,

            Nueva(sofia, "French terry 280 g/m²", "Tela french terry perchada para sudaderas de temporada de invierno.", tela, Prioridad.Alta, 30, -10)
                .Asignar(proveedores.TextilesDelNorte, -9)
                .Aceptar(4, -8)
                .Solicitud,

            Nueva(diego, "Etiqueta de cuidado satinada", "Etiqueta satinada impresa con instrucciones de lavado en español e inglés.", etiqueta, Prioridad.Urgente, 15, -15)
                .Asignar(proveedores.EtiquetasCentroamericanas, -14)
                .Aceptar(-3, -13)
                .Solicitud,

            Nueva(sofia, "Hilo algodón peinado 30/1", "Hilo de algodón peinado 30/1 para tejido de punto en camisetas básicas.", hilo, Prioridad.Alta, 20, -20)
                .Asignar(proveedores.HilosDelValle, -19)
                .Aceptar(-5, -18)
                .Solicitud,

            Nueva(diego, "Rib 1x1 para puños", "Tejido rib 1x1 con elastano para puños y cuellos de sudaderas.", tela, Prioridad.Media, 20, -14)
                .Asignar(proveedores.TextilesDelNorte, -13)
                .Aceptar(-3, -12)
                .Enviar("DHL", "DHL-4821377", -4)
                .Solicitud,

            Nueva(sofia, "Malla de forro transpirable", "Malla de poliéster para forro interno de chaquetas cortavientos.", malla, Prioridad.Media, 25, -18)
                .Asignar(proveedores.MallasDelCaribe, -17)
                .Aceptar(-8, -16)
                .Enviar("Cargo Expreso", "CE-0098812", -6)
                .Solicitud,

            Nueva(diego, "Etiqueta transfer de talla", "Etiqueta transfer termoadherible con talla y composición para prendas sin costura.", etiqueta, Prioridad.Alta, 20, -20)
                .Asignar(proveedores.EtiquetasCentroamericanas, -19)
                .Aceptar(-10, -18)
                .Enviar("FedEx", "FX-77120394", -11)
                .Evaluar(ResultadoEvaluacion.Cumple, "La transferencia resiste 30 lavados sin desprenderse y la tipografía es legible.", 2, -9)
                .Solicitud,

            Nueva(sofia, "Popelina stretch", "Popelina de algodón con 3% de elastano para camisas entalladas, color blanco óptico.", tela, Prioridad.Media, 30, -25)
                .Asignar(proveedores.TextilesDelNorte, -24)
                .Aceptar(-15, -23)
                .Enviar("DHL", "DHL-4790012", -16)
                .Evaluar(ResultadoEvaluacion.CumpleConObservaciones, "Buen tacto y caída; el blanco es levemente más cálido que el estándar.", 3, -14)
                .Aprobar(-13)
                .Solicitud,

            Nueva(diego, "Jersey 180 g/m² algodón", "Jersey 100% algodón peinado para camisetas básicas de la línea juvenil.", tela, Prioridad.Alta, 30, -40)
                .Asignar(proveedores.TextilesDelNorte, -39)
                .Aceptar(-30, -38)
                .Enviar("UPS", "1Z-58H-220341", -31)
                .Evaluar(ResultadoEvaluacion.Cumple, "Gramaje, encogimiento y solidez del color dentro de especificación.", 2, -29)
                .Aprobar(-28)
                .Autorizar(-27)
                .Solicitud,

            Nueva(sofia, "Hilo de coser poliéster 120", "Hilo poliéster 120 para costuras de alta resistencia en ropa de trabajo.", hilo, Prioridad.Alta, 25, -45)
                .Asignar(proveedores.HilosDelValle, -44)
                .Aceptar(-35, -43)
                .Enviar("Cargo Expreso", "CE-0091204", -34)
                .Evaluar(ResultadoEvaluacion.NoCumple, "La resistencia a la tensión quedó 15% por debajo de lo especificado.", 2, -32)
                .RechazarEnAprobacion("Resistencia a la tensión inferior a la especificación técnica.", -31)
                .CambiarFechaCompromiso(-24, -31)
                .Enviar("Cargo Expreso", "CE-0093377", -25)
                .Evaluar(ResultadoEvaluacion.Cumple, "La nueva muestra cumple con la resistencia requerida.", 1, -23)
                .Aprobar(-22)
                .Autorizar(-21)
                .Solicitud,

            Nueva(diego, "Malla 3D para espaldar", "Malla espaciadora 3D para espaldar de mochilas escolares.", malla, Prioridad.Media, 30, -30)
                .Asignar(proveedores.MallasDelCaribe, -29)
                .Aceptar(-20, -28)
                .Enviar("FedEx", "FX-77098811", -21)
                .Evaluar(ResultadoEvaluacion.Cumple, "Espesor y recuperación de la malla dentro de lo solicitado.", 2, -19)
                .Aprobar(-18)
                .RechazarEnAutorizacion("Falta el certificado de composición del proveedor.", -17)
                .CambiarFechaCompromiso(3, -16)
                .Solicitud,

            Nueva(sofia, "Etiqueta bordada para gorra", "Parche bordado con logotipo para el frente de gorras promocionales.", etiqueta, Prioridad.Urgente, 15, -28)
                .Asignar(proveedores.EtiquetasCentroamericanas, -27)
                .Aceptar(-18, -26)
                .Enviar("DHL", "DHL-4775530", -18)
                .Evaluar(ResultadoEvaluacion.NoCumple, "Los colores del hilo no coinciden con la carta de colores aprobada.", 3, -16)
                .RechazarEnAprobacion("Colores fuera de la carta de colores aprobada.", -15)
                .CambiarFechaCompromiso(-6, -14)
                .Solicitud
        ];
    }

    private sealed record ProveedoresEjemplo(
        Proveedor TextilesDelNorte,
        Proveedor HilosDelValle,
        Proveedor EtiquetasCentroamericanas,
        Proveedor MallasDelCaribe)
    {
        public IEnumerable<Proveedor> Todos => [TextilesDelNorte, HilosDelValle, EtiquetasCentroamericanas, MallasDelCaribe];
    }

    private sealed record UsuariosEjemplo(
        Usuario SolicitanteSofia,
        Usuario SolicitanteDiego,
        Usuario Coordinador,
        Usuario Autorizador,
        IReadOnlyDictionary<int, Usuario> PorProveedor)
    {
        public IEnumerable<Usuario> Todos => [SolicitanteSofia, SolicitanteDiego, Coordinador, Autorizador, .. PorProveedor.Values];
    }
}
