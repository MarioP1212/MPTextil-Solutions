# Modelo de datos y flujo de solicitudes

## Diagrama entidad-relación

```mermaid
erDiagram
  ROL ||--o{ USUARIO : asigna
  PROVEEDOR |o--o{ USUARIO : agrupa
  USUARIO ||--o{ SOLICITUD : crea
  PROVEEDOR |o--o{ SOLICITUD : atiende
  TIPO_MATERIAL ||--o{ SOLICITUD : clasifica
  PRIORIDAD ||--o{ SOLICITUD : prioriza
  ESTADO_SOLICITUD ||--o{ SOLICITUD : estado_actual
  SOLICITUD ||--o{ COMPROMISO_ENTREGA : compromete
  PROVEEDOR ||--o{ COMPROMISO_ENTREGA : cumple
  COMPROMISO_ENTREGA ||--o| ENVIO : se_cumple_con
  ENVIO ||--o| EVALUACION : se_evalua
  EVALUACION ||--o{ FOTO_EVALUACION : adjunta
  EVALUACION ||--o{ DECISION : decide
  USUARIO ||--o{ DECISION : toma
  SOLICITUD ||--o{ HISTORIAL_SOLICITUD : registra
  USUARIO ||--o{ HISTORIAL_SOLICITUD : realiza
  ESTADO_SOLICITUD ||--o{ HISTORIAL_SOLICITUD : transicion
  ROL {
    int Id PK
    nvarchar Nombre UK
  }
  USUARIO {
    int Id PK
    nvarchar Nombre
    nvarchar Correo UK
    nvarchar PasswordHash
    int RolId FK
    int ProveedorId FK "solo rol Proveedor"
    bit Activo
    datetime2 FechaCreacion
  }
  PROVEEDOR {
    int Id PK
    nvarchar Nombre UK
    nvarchar Correo
    nvarchar Telefono
    bit Activo
  }
  TIPO_MATERIAL {
    int Id PK
    nvarchar Nombre UK
  }
  PRIORIDAD {
    int Id PK
    nvarchar Nombre UK
  }
  ESTADO_SOLICITUD {
    int Id PK
    nvarchar Nombre UK
  }
  SOLICITUD {
    int Id PK
    nvarchar Codigo UK
    nvarchar NombreMaterial
    nvarchar Descripcion
    int TipoMaterialId FK
    int PrioridadId FK
    int EstadoId FK
    int SolicitanteId FK
    int ProveedorId FK "null hasta Asignado"
    date FechaObjetivo
    datetime2 FechaCreacion
    datetime2 FechaActualizacion
    rowversion Version
  }
  COMPROMISO_ENTREGA {
    int Id PK
    int SolicitudId FK
    int ProveedorId FK
    date FechaCompromiso
    int RegistradoPorId FK
    datetime2 FechaRegistro
    date FechaReemplazo "null si vigente"
  }
  ENVIO {
    int Id PK
    int CompromisoId FK,UK
    date FechaEnvio
    nvarchar Transportista
    nvarchar NumeroGuia
    int RegistradoPorId FK
    datetime2 FechaRegistro
  }
  EVALUACION {
    int Id PK
    int EnvioId FK,UK
    int Resultado "Cumple, No cumple, Con observaciones"
    nvarchar Comentarios
    int EvaluadoPorId FK
    datetime2 FechaEvaluacion
  }
  FOTO_EVALUACION {
    int Id PK
    int EvaluacionId FK
    nvarchar BlobNombre UK
    nvarchar NombreOriginal
    nvarchar ContentType
    bigint TamanoBytes
    datetime2 FechaSubida
  }
  DECISION {
    int Id PK
    int EvaluacionId FK
    int Nivel "1 Aprobacion, 2 Autorizacion"
    bit Aprobada
    nvarchar Motivo "obligatorio si rechaza"
    int UsuarioId FK
    datetime2 Fecha
  }
  HISTORIAL_SOLICITUD {
    bigint Id PK
    int SolicitudId FK
    int UsuarioId FK
    int EstadoAnteriorId FK
    int EstadoNuevoId FK
    nvarchar Accion
    nvarchar Detalle
    datetime2 Fecha
  }
```

## Flujo de estados

```mermaid
stateDiagram-v2
  state "Solicitado" as Solicitado
  state "Asignado" as Asignado
  state "En progreso" as EnProgreso
  state "Enviado" as Enviado
  state "En evaluación" as EnEvaluacion
  state "Pendiente de autorización" as PendienteAutorizacion
  state "Aprobado" as Aprobado
  [*] --> Solicitado : Solicitante crea
  Solicitado --> Asignado : Coordinador asigna proveedor
  Asignado --> Solicitado : Proveedor declina con motivo
  Asignado --> EnProgreso : Proveedor acepta con fecha compromiso
  EnProgreso --> Enviado : Proveedor registra envío
  Enviado --> EnEvaluacion : Coordinador registra evaluación y fotos
  EnEvaluacion --> PendienteAutorizacion : Solicitante creador aprueba
  EnEvaluacion --> EnProgreso : Solicitante creador rechaza
  PendienteAutorizacion --> Aprobado : Autorizador autoriza
  PendienteAutorizacion --> EnProgreso : Autorizador rechaza
  Aprobado --> [*]
  note right of EnProgreso : Vencida si pasa la fecha compromiso sin envío
```

## Roles

| Rol | Responsabilidad |
|---|---|
| Solicitante | Crea solicitudes y aprueba o rechaza la muestra de las solicitudes que creó. |
| Coordinador | Asigna la solicitud a un proveedor y registra la evaluación con fotos. |
| Proveedor | Acepta con fecha compromiso o declina, y registra el envío. Solo ve sus solicitudes. |
| Autorizador | Da la autorización final después de la aprobación del Solicitante. |

## Reglas de negocio

1. Solo se permiten las transiciones del diagrama y únicamente por el rol indicado.
2. El proveedor solo actúa sobre solicitudes asignadas a su empresa y el solicitante solo decide sobre las solicitudes que creó.
3. Rechazado no es un estado de reposo: la decisión de rechazo regresa la solicitud a En progreso y queda registrada en `DECISION` y en el historial.
4. El motivo es obligatorio al declinar una asignación y al rechazar una muestra.
5. Una solicitud aceptada por el proveedor ya no se puede reasignar.
6. La fecha objetivo y la fecha compromiso no pueden ser anteriores a hoy; la fecha de envío no puede ser posterior a hoy.
7. Ninguna fecha compromiso se sobrescribe: cada cambio crea un registro nuevo en `COMPROMISO_ENTREGA` y marca el anterior con `FechaReemplazo`.
8. Tras un rechazo, si la fecha compromiso aún no pasa se conserva y el proveedor puede cambiarla; si ya pasó, debe registrar una nueva antes de reenviar.
9. El proveedor solo puede cambiar la fecha compromiso después de un rechazo.
10. La evaluación requiere resultado, comentarios y de 1 a 5 fotos JPG, PNG o WEBP de hasta 5 MB cada una.
11. Cada cambio de la solicitud queda en `HISTORIAL_SOLICITUD` con usuario, acción, detalle y fecha.
12. Las fechas de auditoría se guardan en UTC; las fechas de negocio (objetivo, compromiso, envío y reemplazo) se guardan como fecha local sin hora.

## Indicadores

| Indicador | Cálculo |
|---|---|
| Solicitudes por estado | Conteo de solicitudes agrupado por estado actual. |
| Solicitudes vencidas | Solicitudes en En progreso cuyo compromiso vigente tiene fecha menor a hoy. |
| Entrega a tiempo | Envío con `FechaEnvio` menor o igual a la `FechaCompromiso` de su compromiso. |
| Compromiso vencido sin envío | Compromiso sin envío cuya fecha es menor a `FechaReemplazo`, o a hoy si sigue vigente. |
| % de entregas a tiempo por proveedor | Envíos a tiempo ÷ (envíos a tiempo + envíos tarde + compromisos vencidos sin envío). |
