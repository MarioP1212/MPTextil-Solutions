using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace MPTextil.Infrastructure.Persistencia.Migraciones
{
    public partial class Inicial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstadoSolicitud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadoSolicitud", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prioridad",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prioridad", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rol", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TipoMaterial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoMaterial", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RolId = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: true),
                    Activo = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                    table.CheckConstraint("CK_Usuario_ProveedorSegunRol", "([RolId] = 3 AND [ProveedorId] IS NOT NULL) OR ([RolId] <> 3 AND [ProveedorId] IS NULL)");
                    table.ForeignKey(
                        name: "FK_Usuario_Proveedor_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Usuario_Rol_RolId",
                        column: x => x.RolId,
                        principalTable: "Rol",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Solicitud",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, computedColumnSql: "'SOL-' + CASE WHEN [Id] < 100000 THEN RIGHT('00000' + CAST([Id] AS varchar(10)), 5) ELSE CAST([Id] AS varchar(10)) END", stored: true),
                    NombreMaterial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    TipoMaterialId = table.Column<int>(type: "int", nullable: false),
                    PrioridadId = table.Column<int>(type: "int", nullable: false),
                    EstadoId = table.Column<int>(type: "int", nullable: false),
                    SolicitanteId = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: true),
                    FechaObjetivo = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Solicitud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Solicitud_EstadoSolicitud_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "EstadoSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitud_Prioridad_PrioridadId",
                        column: x => x.PrioridadId,
                        principalTable: "Prioridad",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitud_Proveedor_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitud_TipoMaterial_TipoMaterialId",
                        column: x => x.TipoMaterialId,
                        principalTable: "TipoMaterial",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Solicitud_Usuario_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CompromisoEntrega",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    FechaCompromiso = table.Column<DateOnly>(type: "date", nullable: false),
                    RegistradoPorId = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaReemplazo = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompromisoEntrega", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompromisoEntrega_Proveedor_ProveedorId",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompromisoEntrega_Solicitud_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompromisoEntrega_Usuario_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HistorialSolicitud",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SolicitudId = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    EstadoAnteriorId = table.Column<int>(type: "int", nullable: true),
                    EstadoNuevoId = table.Column<int>(type: "int", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Detalle = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialSolicitud", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialSolicitud_EstadoSolicitud_EstadoAnteriorId",
                        column: x => x.EstadoAnteriorId,
                        principalTable: "EstadoSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialSolicitud_EstadoSolicitud_EstadoNuevoId",
                        column: x => x.EstadoNuevoId,
                        principalTable: "EstadoSolicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HistorialSolicitud_Solicitud_SolicitudId",
                        column: x => x.SolicitudId,
                        principalTable: "Solicitud",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistorialSolicitud_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Envio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompromisoId = table.Column<int>(type: "int", nullable: false),
                    FechaEnvio = table.Column<DateOnly>(type: "date", nullable: false),
                    Transportista = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NumeroGuia = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RegistradoPorId = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Envio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Envio_CompromisoEntrega_CompromisoId",
                        column: x => x.CompromisoId,
                        principalTable: "CompromisoEntrega",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Envio_Usuario_RegistradoPorId",
                        column: x => x.RegistradoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Evaluacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EnvioId = table.Column<int>(type: "int", nullable: false),
                    Resultado = table.Column<int>(type: "int", nullable: false),
                    Comentarios = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    EvaluadoPorId = table.Column<int>(type: "int", nullable: false),
                    FechaEvaluacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Evaluacion", x => x.Id);
                    table.CheckConstraint("CK_Evaluacion_Resultado", "[Resultado] IN (1, 2, 3)");
                    table.ForeignKey(
                        name: "FK_Evaluacion_Envio_EnvioId",
                        column: x => x.EnvioId,
                        principalTable: "Envio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Evaluacion_Usuario_EvaluadoPorId",
                        column: x => x.EvaluadoPorId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Decision",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluacionId = table.Column<int>(type: "int", nullable: false),
                    Nivel = table.Column<int>(type: "int", nullable: false),
                    Aprobada = table.Column<bool>(type: "bit", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Decision", x => x.Id);
                    table.CheckConstraint("CK_Decision_MotivoAlRechazar", "[Aprobada] = 1 OR [Motivo] IS NOT NULL");
                    table.CheckConstraint("CK_Decision_Nivel", "[Nivel] IN (1, 2)");
                    table.ForeignKey(
                        name: "FK_Decision_Evaluacion_EvaluacionId",
                        column: x => x.EvaluacionId,
                        principalTable: "Evaluacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Decision_Usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FotoEvaluacion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvaluacionId = table.Column<int>(type: "int", nullable: false),
                    BlobNombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NombreOriginal = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TamanoBytes = table.Column<long>(type: "bigint", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FotoEvaluacion", x => x.Id);
                    table.CheckConstraint("CK_FotoEvaluacion_Tamano", "[TamanoBytes] > 0 AND [TamanoBytes] <= 5242880");
                    table.ForeignKey(
                        name: "FK_FotoEvaluacion_Evaluacion_EvaluacionId",
                        column: x => x.EvaluacionId,
                        principalTable: "Evaluacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "EstadoSolicitud",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Solicitado" },
                    { 2, "Asignado" },
                    { 3, "En progreso" },
                    { 4, "Enviado" },
                    { 5, "En evaluación" },
                    { 6, "Pendiente de autorización" },
                    { 7, "Aprobado" }
                });

            migrationBuilder.InsertData(
                table: "Prioridad",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Baja" },
                    { 2, "Media" },
                    { 3, "Alta" },
                    { 4, "Urgente" }
                });

            migrationBuilder.InsertData(
                table: "Rol",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { 1, "Solicitante" },
                    { 2, "Coordinador" },
                    { 3, "Proveedor" },
                    { 4, "Autorizador" }
                });

            migrationBuilder.InsertData(
                table: "TipoMaterial",
                columns: new[] { "Id", "Activo", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Tela" },
                    { 2, true, "Malla" },
                    { 3, true, "Etiqueta" },
                    { 4, true, "Hilo" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompromisoEntrega_ProveedorId_FechaCompromiso",
                table: "CompromisoEntrega",
                columns: new[] { "ProveedorId", "FechaCompromiso" });

            migrationBuilder.CreateIndex(
                name: "IX_CompromisoEntrega_RegistradoPorId",
                table: "CompromisoEntrega",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_CompromisoEntrega_SolicitudId",
                table: "CompromisoEntrega",
                column: "SolicitudId");

            migrationBuilder.CreateIndex(
                name: "IX_Decision_EvaluacionId_Nivel",
                table: "Decision",
                columns: new[] { "EvaluacionId", "Nivel" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Decision_UsuarioId",
                table: "Decision",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Envio_CompromisoId",
                table: "Envio",
                column: "CompromisoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Envio_RegistradoPorId",
                table: "Envio",
                column: "RegistradoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_EstadoSolicitud_Nombre",
                table: "EstadoSolicitud",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evaluacion_EnvioId",
                table: "Evaluacion",
                column: "EnvioId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Evaluacion_EvaluadoPorId",
                table: "Evaluacion",
                column: "EvaluadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_FotoEvaluacion_BlobNombre",
                table: "FotoEvaluacion",
                column: "BlobNombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FotoEvaluacion_EvaluacionId",
                table: "FotoEvaluacion",
                column: "EvaluacionId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialSolicitud_EstadoAnteriorId",
                table: "HistorialSolicitud",
                column: "EstadoAnteriorId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialSolicitud_EstadoNuevoId",
                table: "HistorialSolicitud",
                column: "EstadoNuevoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialSolicitud_SolicitudId_Fecha",
                table: "HistorialSolicitud",
                columns: new[] { "SolicitudId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialSolicitud_UsuarioId",
                table: "HistorialSolicitud",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Prioridad_Nombre",
                table: "Prioridad",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proveedor_Nombre",
                table: "Proveedor",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rol_Nombre",
                table: "Rol",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_Codigo",
                table: "Solicitud",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_EstadoId",
                table: "Solicitud",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_PrioridadId",
                table: "Solicitud",
                column: "PrioridadId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_ProveedorId",
                table: "Solicitud",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_SolicitanteId",
                table: "Solicitud",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Solicitud_TipoMaterialId",
                table: "Solicitud",
                column: "TipoMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_TipoMaterial_Nombre",
                table: "TipoMaterial",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_Correo",
                table: "Usuario",
                column: "Correo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_ProveedorId",
                table: "Usuario",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_RolId",
                table: "Usuario",
                column: "RolId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Decision");

            migrationBuilder.DropTable(
                name: "FotoEvaluacion");

            migrationBuilder.DropTable(
                name: "HistorialSolicitud");

            migrationBuilder.DropTable(
                name: "Evaluacion");

            migrationBuilder.DropTable(
                name: "Envio");

            migrationBuilder.DropTable(
                name: "CompromisoEntrega");

            migrationBuilder.DropTable(
                name: "Solicitud");

            migrationBuilder.DropTable(
                name: "EstadoSolicitud");

            migrationBuilder.DropTable(
                name: "Prioridad");

            migrationBuilder.DropTable(
                name: "TipoMaterial");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Proveedor");

            migrationBuilder.DropTable(
                name: "Rol");
        }
    }
}
