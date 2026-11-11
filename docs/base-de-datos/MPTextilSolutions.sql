IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [EstadoSolicitud] (
        [Id] int NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_EstadoSolicitud] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [Prioridad] (
        [Id] int NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Prioridad] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [Proveedor] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(150) NOT NULL,
        [Correo] nvarchar(150) NOT NULL,
        [Telefono] nvarchar(30) NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_Proveedor] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [Rol] (
        [Id] int NOT NULL,
        [Nombre] nvarchar(50) NOT NULL,
        CONSTRAINT [PK_Rol] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [TipoMaterial] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(50) NOT NULL,
        [Activo] bit NOT NULL,
        CONSTRAINT [PK_TipoMaterial] PRIMARY KEY ([Id])
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [Usuario] (
        [Id] int NOT NULL IDENTITY,
        [Nombre] nvarchar(100) NOT NULL,
        [Correo] nvarchar(150) NOT NULL,
        [PasswordHash] nvarchar(500) NOT NULL,
        [RolId] int NOT NULL,
        [ProveedorId] int NULL,
        [Activo] bit NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Usuario] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Usuario_ProveedorSegunRol] CHECK (([RolId] = 3 AND [ProveedorId] IS NOT NULL) OR ([RolId] <> 3 AND [ProveedorId] IS NULL)),
        CONSTRAINT [FK_Usuario_Proveedor_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedor] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Usuario_Rol_RolId] FOREIGN KEY ([RolId]) REFERENCES [Rol] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [Solicitud] (
        [Id] int NOT NULL IDENTITY,
        [Codigo] AS 'SOL-' + CASE WHEN [Id] < 100000 THEN RIGHT('00000' + CAST([Id] AS varchar(10)), 5) ELSE CAST([Id] AS varchar(10)) END PERSISTED,
        [NombreMaterial] nvarchar(150) NOT NULL,
        [Descripcion] nvarchar(1000) NOT NULL,
        [TipoMaterialId] int NOT NULL,
        [PrioridadId] int NOT NULL,
        [EstadoId] int NOT NULL,
        [SolicitanteId] int NOT NULL,
        [ProveedorId] int NULL,
        [FechaObjetivo] date NOT NULL,
        [FechaCreacion] datetime2 NOT NULL,
        [FechaActualizacion] datetime2 NOT NULL,
        [Version] rowversion NULL,
        CONSTRAINT [PK_Solicitud] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Solicitud_EstadoSolicitud_EstadoId] FOREIGN KEY ([EstadoId]) REFERENCES [EstadoSolicitud] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitud_Prioridad_PrioridadId] FOREIGN KEY ([PrioridadId]) REFERENCES [Prioridad] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitud_Proveedor_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedor] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitud_TipoMaterial_TipoMaterialId] FOREIGN KEY ([TipoMaterialId]) REFERENCES [TipoMaterial] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Solicitud_Usuario_SolicitanteId] FOREIGN KEY ([SolicitanteId]) REFERENCES [Usuario] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [CompromisoEntrega] (
        [Id] int NOT NULL IDENTITY,
        [SolicitudId] int NOT NULL,
        [ProveedorId] int NOT NULL,
        [FechaCompromiso] date NOT NULL,
        [RegistradoPorId] int NOT NULL,
        [FechaRegistro] datetime2 NOT NULL,
        [FechaReemplazo] date NULL,
        CONSTRAINT [PK_CompromisoEntrega] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_CompromisoEntrega_Proveedor_ProveedorId] FOREIGN KEY ([ProveedorId]) REFERENCES [Proveedor] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_CompromisoEntrega_Solicitud_SolicitudId] FOREIGN KEY ([SolicitudId]) REFERENCES [Solicitud] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CompromisoEntrega_Usuario_RegistradoPorId] FOREIGN KEY ([RegistradoPorId]) REFERENCES [Usuario] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [HistorialSolicitud] (
        [Id] bigint NOT NULL IDENTITY,
        [SolicitudId] int NOT NULL,
        [UsuarioId] int NOT NULL,
        [EstadoAnteriorId] int NULL,
        [EstadoNuevoId] int NOT NULL,
        [Accion] nvarchar(40) NOT NULL,
        [Detalle] nvarchar(1000) NOT NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_HistorialSolicitud] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_HistorialSolicitud_EstadoSolicitud_EstadoAnteriorId] FOREIGN KEY ([EstadoAnteriorId]) REFERENCES [EstadoSolicitud] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistorialSolicitud_EstadoSolicitud_EstadoNuevoId] FOREIGN KEY ([EstadoNuevoId]) REFERENCES [EstadoSolicitud] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_HistorialSolicitud_Solicitud_SolicitudId] FOREIGN KEY ([SolicitudId]) REFERENCES [Solicitud] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_HistorialSolicitud_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [Envio] (
        [Id] int NOT NULL IDENTITY,
        [CompromisoId] int NOT NULL,
        [FechaEnvio] date NOT NULL,
        [Transportista] nvarchar(100) NOT NULL,
        [NumeroGuia] nvarchar(50) NOT NULL,
        [RegistradoPorId] int NOT NULL,
        [FechaRegistro] datetime2 NOT NULL,
        CONSTRAINT [PK_Envio] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Envio_CompromisoEntrega_CompromisoId] FOREIGN KEY ([CompromisoId]) REFERENCES [CompromisoEntrega] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Envio_Usuario_RegistradoPorId] FOREIGN KEY ([RegistradoPorId]) REFERENCES [Usuario] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [Evaluacion] (
        [Id] int NOT NULL IDENTITY,
        [EnvioId] int NOT NULL,
        [Resultado] int NOT NULL,
        [Comentarios] nvarchar(1000) NOT NULL,
        [EvaluadoPorId] int NOT NULL,
        [FechaEvaluacion] datetime2 NOT NULL,
        CONSTRAINT [PK_Evaluacion] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Evaluacion_Resultado] CHECK ([Resultado] IN (1, 2, 3)),
        CONSTRAINT [FK_Evaluacion_Envio_EnvioId] FOREIGN KEY ([EnvioId]) REFERENCES [Envio] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Evaluacion_Usuario_EvaluadoPorId] FOREIGN KEY ([EvaluadoPorId]) REFERENCES [Usuario] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [Decision] (
        [Id] int NOT NULL IDENTITY,
        [EvaluacionId] int NOT NULL,
        [Nivel] int NOT NULL,
        [Aprobada] bit NOT NULL,
        [Motivo] nvarchar(500) NULL,
        [UsuarioId] int NOT NULL,
        [Fecha] datetime2 NOT NULL,
        CONSTRAINT [PK_Decision] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Decision_MotivoAlRechazar] CHECK ([Aprobada] = 1 OR [Motivo] IS NOT NULL),
        CONSTRAINT [CK_Decision_Nivel] CHECK ([Nivel] IN (1, 2)),
        CONSTRAINT [FK_Decision_Evaluacion_EvaluacionId] FOREIGN KEY ([EvaluacionId]) REFERENCES [Evaluacion] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Decision_Usuario_UsuarioId] FOREIGN KEY ([UsuarioId]) REFERENCES [Usuario] ([Id]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE TABLE [FotoEvaluacion] (
        [Id] int NOT NULL IDENTITY,
        [EvaluacionId] int NOT NULL,
        [BlobNombre] nvarchar(200) NOT NULL,
        [NombreOriginal] nvarchar(200) NOT NULL,
        [ContentType] nvarchar(100) NOT NULL,
        [TamanoBytes] bigint NOT NULL,
        [FechaSubida] datetime2 NOT NULL,
        CONSTRAINT [PK_FotoEvaluacion] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_FotoEvaluacion_Tamano] CHECK ([TamanoBytes] > 0 AND [TamanoBytes] <= 5242880),
        CONSTRAINT [FK_FotoEvaluacion_Evaluacion_EvaluacionId] FOREIGN KEY ([EvaluacionId]) REFERENCES [Evaluacion] ([Id]) ON DELETE CASCADE
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[EstadoSolicitud]'))
        SET IDENTITY_INSERT [EstadoSolicitud] ON;
    EXEC(N'INSERT INTO [EstadoSolicitud] ([Id], [Nombre])
    VALUES (1, N''Solicitado''),
    (2, N''Asignado''),
    (3, N''En progreso''),
    (4, N''Enviado''),
    (5, N''En evaluación''),
    (6, N''Pendiente de autorización''),
    (7, N''Aprobado'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[EstadoSolicitud]'))
        SET IDENTITY_INSERT [EstadoSolicitud] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Prioridad]'))
        SET IDENTITY_INSERT [Prioridad] ON;
    EXEC(N'INSERT INTO [Prioridad] ([Id], [Nombre])
    VALUES (1, N''Baja''),
    (2, N''Media''),
    (3, N''Alta''),
    (4, N''Urgente'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Prioridad]'))
        SET IDENTITY_INSERT [Prioridad] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Rol]'))
        SET IDENTITY_INSERT [Rol] ON;
    EXEC(N'INSERT INTO [Rol] ([Id], [Nombre])
    VALUES (1, N''Solicitante''),
    (2, N''Coordinador''),
    (3, N''Proveedor''),
    (4, N''Autorizador'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Nombre') AND [object_id] = OBJECT_ID(N'[Rol]'))
        SET IDENTITY_INSERT [Rol] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Nombre') AND [object_id] = OBJECT_ID(N'[TipoMaterial]'))
        SET IDENTITY_INSERT [TipoMaterial] ON;
    EXEC(N'INSERT INTO [TipoMaterial] ([Id], [Activo], [Nombre])
    VALUES (1, CAST(1 AS bit), N''Tela''),
    (2, CAST(1 AS bit), N''Malla''),
    (3, CAST(1 AS bit), N''Etiqueta''),
    (4, CAST(1 AS bit), N''Hilo'')');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Activo', N'Nombre') AND [object_id] = OBJECT_ID(N'[TipoMaterial]'))
        SET IDENTITY_INSERT [TipoMaterial] OFF;
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_CompromisoEntrega_ProveedorId_FechaCompromiso] ON [CompromisoEntrega] ([ProveedorId], [FechaCompromiso]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_CompromisoEntrega_RegistradoPorId] ON [CompromisoEntrega] ([RegistradoPorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_CompromisoEntrega_SolicitudId] ON [CompromisoEntrega] ([SolicitudId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Decision_EvaluacionId_Nivel] ON [Decision] ([EvaluacionId], [Nivel]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_Decision_UsuarioId] ON [Decision] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Envio_CompromisoId] ON [Envio] ([CompromisoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_Envio_RegistradoPorId] ON [Envio] ([RegistradoPorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_EstadoSolicitud_Nombre] ON [EstadoSolicitud] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Evaluacion_EnvioId] ON [Evaluacion] ([EnvioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_Evaluacion_EvaluadoPorId] ON [Evaluacion] ([EvaluadoPorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_FotoEvaluacion_BlobNombre] ON [FotoEvaluacion] ([BlobNombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_FotoEvaluacion_EvaluacionId] ON [FotoEvaluacion] ([EvaluacionId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_HistorialSolicitud_EstadoAnteriorId] ON [HistorialSolicitud] ([EstadoAnteriorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_HistorialSolicitud_EstadoNuevoId] ON [HistorialSolicitud] ([EstadoNuevoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_HistorialSolicitud_SolicitudId_Fecha] ON [HistorialSolicitud] ([SolicitudId], [Fecha]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_HistorialSolicitud_UsuarioId] ON [HistorialSolicitud] ([UsuarioId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Prioridad_Nombre] ON [Prioridad] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Proveedor_Nombre] ON [Proveedor] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Rol_Nombre] ON [Rol] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Solicitud_Codigo] ON [Solicitud] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_Solicitud_EstadoId] ON [Solicitud] ([EstadoId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_Solicitud_PrioridadId] ON [Solicitud] ([PrioridadId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_Solicitud_ProveedorId] ON [Solicitud] ([ProveedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_Solicitud_SolicitanteId] ON [Solicitud] ([SolicitanteId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_Solicitud_TipoMaterialId] ON [Solicitud] ([TipoMaterialId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_TipoMaterial_Nombre] ON [TipoMaterial] ([Nombre]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Usuario_Correo] ON [Usuario] ([Correo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_Usuario_ProveedorId] ON [Usuario] ([ProveedorId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    CREATE INDEX [IX_Usuario_RolId] ON [Usuario] ([RolId]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261111031802_Inicial'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261111031802_Inicial', N'10.0.15');
END;

COMMIT;
GO

