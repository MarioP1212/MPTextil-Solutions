# MPTextil Solutions

Portal para dar seguimiento a las solicitudes de desarrollo de materiales textiles (telas, mallas, etiquetas e hilos) desde que se piden hasta que la muestra se aprueba o se rechaza.

- **API** en .NET 10 con Swagger, autenticación JWT por rol y errores en formato ProblemDetails.
- **Front** en Blazor WebAssembly con MudBlazor.
- **SQL Server** con Entity Framework Core (migraciones y datos de ejemplo automáticos).
- **Fotos** en Azure Blob Storage (Azurite en local).

---

## Contenido

1. [Tecnologías](#tecnologías)
2. [Estructura de la solución](#estructura-de-la-solución)
3. [Requisitos](#requisitos)
4. [Correr el proyecto en local](#correr-el-proyecto-en-local)
5. [Usuarios de ejemplo](#usuarios-de-ejemplo)
6. [Datos de ejemplo](#datos-de-ejemplo)
7. [Flujo de una solicitud](#flujo-de-una-solicitud)
8. [Endpoints de la API](#endpoints-de-la-api)
9. [Pruebas](#pruebas)
10. [Decisiones de diseño](#decisiones-de-diseño)
11. [Qué quedó fuera y por qué](#qué-quedó-fuera-y-por-qué)
12. [Problemas comunes](#problemas-comunes)

---

## Tecnologías

| Capa | Tecnología |
|---|---|
| Backend | .NET 10, ASP.NET Core Web API (controladores) |
| Datos | SQL Server, Entity Framework Core 10 |
| Validación | FluentValidation 12 |
| Seguridad | JWT (HMAC-SHA256), contraseñas con PBKDF2-SHA256 |
| Documentación | Swagger (Swashbuckle 10) |
| Archivos | Azure Blob Storage / Azurite |
| Frontend | Blazor WebAssembly, MudBlazor 9 |
| Pruebas | xUnit |

## Estructura de la solución

```
MPTextil-Solutions/
├── MPTextilSolutions.slnx          Solución
├── global.json                     Fija el SDK de .NET 10
├── nuget.config                    Usa solo nuget.org
├── Directory.Build.props           Propiedades comunes de los proyectos
├── Directory.Packages.props        Versiones centralizadas de paquetes
├── dotnet-tools.json               Herramienta local dotnet-ef
├── docs/
│   ├── modelo-datos.md             Diagrama entidad-relación, flujo, reglas e indicadores
│   └── base-de-datos/
│       └── MPTextilSolutions.sql   Script idempotente del esquema
├── src/
│   ├── MPTextil.Domain             Entidades, reglas de negocio y máquina de estados
│   ├── MPTextil.Application        Casos de uso, validadores y contratos (repositorios, almacenamiento, reloj)
│   ├── MPTextil.Infrastructure     EF Core, repositorios, Unit of Work, JWT, Blob Storage, datos de ejemplo
│   ├── MPTextil.Api                Controladores, seguridad, Swagger y manejo de errores
│   └── MPTextil.Web                Front en Blazor WebAssembly
└── tests/
    ├── MPTextil.Tests              Pruebas unitarias con xUnit
    └── MPTextil.Tests.Integracion  Pruebas de integración de la API por HTTP
```

`Domain` no depende de nada. `Application` depende de `Domain` y define los contratos (repositorios, almacenamiento, reloj, tokens). `Infrastructure` implementa esos contratos. `Api` usa los casos de uso de `Application` y solo referencia `Infrastructure` para registrar sus servicios. El front (`Web`) es independiente y se comunica con la API por HTTP.

## Requisitos

| Requisito | Detalle |
|---|---|
| .NET SDK | 10.0.300 o superior. Verifica con `dotnet --list-sdks` |
| SQL Server | LocalDB (viene con Visual Studio), Express o Developer |
| Azurite | Emulador de Azure Storage para guardar las fotos en local |
| Navegador | Cualquiera moderno |

Visual Studio 2022/2026 con la carga de trabajo **Desarrollo de ASP.NET y web** trae Azurite y normalmente también LocalDB (componente *SQL Server Express LocalDB*). Si no usas Visual Studio, Azurite se instala con Node.js:

```bash
npm install -g azurite
```

## Correr el proyecto en local

Se necesitan tres procesos corriendo al mismo tiempo: **Azurite**, la **API** y el **front**. Los comandos se ejecutan desde la carpeta raíz `MPTextil-Solutions`.

### 1. Base de datos

**No hay que crearla a mano.** Al arrancar, la API aplica las migraciones, crea la base `MPTextilSolutions` si no existe y siembra los datos de ejemplo.

Solo revisa la cadena de conexión en `src/MPTextil.Api/appsettings.Development.json`:

```json
"ConnectionStrings": {
  "MPTextil": "Server=(localdb)\\MSSQLLocalDB;Database=MPTextilSolutions;Trusted_Connection=True;TrustServerCertificate=True"
}
```

Si usas otra instancia de SQL Server, cambia solo el `Server`. Ejemplos:

| Instancia | Valor de `Server` |
|---|---|
| LocalDB | `(localdb)\\MSSQLLocalDB` |
| SQL Server Express | `.\\SQLEXPRESS` |
| SQL Server Developer (instancia por defecto) | `localhost` |

Si entras a SQL Server con usuario y contraseña en lugar de la sesión de Windows:

```json
"MPTextil": "Server=localhost;Database=MPTextilSolutions;User Id=tu_usuario;Password=tu_contraseña;TrustServerCertificate=True"
```

> **Alternativa manual:** crea una base vacía `MPTextilSolutions` y ejecuta `docs/base-de-datos/MPTextilSolutions.sql` en SQL Server Management Studio. Al arrancar, la API detecta que el esquema ya existe y solo siembra los datos de ejemplo.

### 2. Azurite (fotos)

Abre una terminal y déjala corriendo.

Con Azurite instalado por npm:

```bash
azurite --silent --location ./.azurite --skipApiVersionCheck
```

Con el Azurite que trae Visual Studio (ajusta la versión y edición de tu instalación):

```bash
"C:\Program Files\Microsoft Visual Studio\18\Professional\Common7\IDE\Extensions\Microsoft\Azure Storage Emulator\azurite.exe" --silent --location ./.azurite --skipApiVersionCheck
```

Azurite escucha en `http://127.0.0.1:10000`. La API crea sola el contenedor `fotos-muestras` y sube las fotos de los datos de ejemplo.

### 3. API

En otra terminal:

```bash
dotnet run --project src/MPTextil.Api --launch-profile http
```

- Swagger: <http://localhost:5236/swagger>
- La primera vez tarda unos segundos más porque crea la base y siembra los datos.

### 4. Front

En una tercera terminal:

```bash
dotnet run --project src/MPTextil.Web --launch-profile http
```

- Portal: <http://localhost:5240>
- El front llama a la API en `http://localhost:5236/` (configurado en `src/MPTextil.Web/wwwroot/appsettings.Development.json`).

### Desde Visual Studio

1. Abre `MPTextilSolutions.slnx`.
2. Clic derecho en la solución → **Configurar proyectos de inicio** → **Varios proyectos de inicio** → marca `MPTextil.Api` y `MPTextil.Web` como **Iniciar**.
3. Inicia Azurite (sección 2) y presiona **F5**.

### Probar la API desde Swagger

1. Abre <http://localhost:5236/swagger>.
2. Ejecuta `POST /api/autenticacion/iniciar-sesion` con un usuario de ejemplo.
3. Copia el valor de `token`, pulsa **Authorize** y pégalo.
4. Ya puedes usar todos los endpoints con los permisos de ese rol.

## Usuarios de ejemplo

Todos usan la contraseña **`MPTextil2026!`** (configurable en `DatosEjemplo:Contrasena`).

| Rol | Nombre | Correo |
|---|---|---|
| Solicitante | Sofía Martínez | `sofia.martinez@mptextil.example` |
| Solicitante | Diego Ramírez | `diego.ramirez@mptextil.example` |
| Coordinador | Carlos Mejía | `carlos.mejia@mptextil.example` |
| Autorizador | Ana Castillo | `ana.castillo@mptextil.example` |
| Proveedor · Textiles del Norte | Pedro López | `pedro.lopez@textilesdelnorte.example` |
| Proveedor · Hilos y Fibras del Valle | Lucía Reyes | `lucia.reyes@hilosdelvalle.example` |
| Proveedor · Etiquetas Centroamericanas | Jorge Flores | `jorge.flores@etiquetasca.example` |
| Proveedor · Mallas Técnicas del Caribe | Marta Gómez | `marta.gomez@mallascaribe.example` |

Los correos usan el dominio reservado `.example`, así que son ficticios.

## Datos de ejemplo

Se crean la primera vez que arranca la API, usando las mismas reglas del dominio que usa la aplicación. Las fechas son relativas al día en que se siembran, así que los datos siempre se ven recientes.

- **4 proveedores**, cada uno con su usuario.
- **15 solicitudes** en todos los estados:

| Estado | Cantidad | Casos que muestran |
|---|---|---|
| Solicitado | 2 | Recién creadas |
| Asignado | 2 | Una declinada por un proveedor y reasignada a otro |
| En progreso | 5 | 3 vencidas; una regresó tras un rechazo del autorizador |
| Enviado | 2 | Una a tiempo y otra tarde |
| En evaluación | 1 | Esperando la decisión del solicitante |
| Pendiente de autorización | 1 | Esperando al autorizador |
| Aprobado | 2 | Una directa y otra aprobada después de un rechazo y un reenvío |

- **Fotos de muestra** de tela, malla, etiqueta e hilo en cada evaluación. Si el almacenamiento está vacío (por ejemplo, en otro equipo), la API las vuelve a subir al arrancar.

## Flujo de una solicitud

| Estado | Quién actúa | Acción |
|---|---|---|
| Solicitado | Solicitante | Crea la solicitud |
| Asignado | Coordinador | Asigna un proveedor (el proveedor puede declinar y regresa a Solicitado) |
| En progreso | Proveedor | Acepta con fecha compromiso. Si la fecha pasa sin envío, queda **vencida** |
| Enviado | Proveedor | Registra fecha de envío, transportista y guía |
| En evaluación | Coordinador | Registra resultado, comentarios y de 1 a 5 fotos |
| Pendiente de autorización | Solicitante (creador) | Aprueba la muestra; si la rechaza, regresa a En progreso |
| Aprobado | Autorizador | Da la autorización final; si la rechaza, regresa a En progreso |

El diagrama entidad-relación, el diagrama de estados, las reglas de negocio completas y la fórmula de los indicadores están en [`docs/modelo-datos.md`](docs/modelo-datos.md).

## Endpoints de la API

Todos requieren token salvo el inicio de sesión.

| Método | Ruta | Rol |
|---|---|---|
| POST | `/api/autenticacion/iniciar-sesion` | Público |
| GET | `/api/autenticacion/perfil` | Cualquiera |
| GET | `/api/catalogos/proveedores` · `tipos-material` · `prioridades` · `estados` | Cualquiera |
| GET | `/api/indicadores` | Cualquiera (el proveedor solo ve lo suyo) |
| GET | `/api/solicitudes` | Cualquiera (el proveedor solo ve las suyas) |
| GET | `/api/solicitudes/{estado}` | Igual que el anterior, filtrado por estado |
| GET | `/api/solicitudes/vencidas` | Igual que el anterior |
| GET | `/api/solicitudes/{id}` | Igual que el anterior |
| POST | `/api/solicitudes` | Solicitante |
| POST | `/api/solicitudes/{id}/asignar` | Coordinador |
| POST | `/api/solicitudes/{id}/declinar` | Proveedor |
| POST | `/api/solicitudes/{id}/aceptar` | Proveedor |
| POST | `/api/solicitudes/{id}/cambiar-fecha-compromiso` | Proveedor |
| POST | `/api/solicitudes/{id}/registrar-envio` | Proveedor |
| POST | `/api/solicitudes/{id}/registrar-evaluacion` | Coordinador (multipart con fotos) |
| POST | `/api/solicitudes/{id}/aprobar` | Solicitante |
| POST | `/api/solicitudes/{id}/autorizar` | Autorizador |
| POST | `/api/solicitudes/{id}/rechazar` | Solicitante o Autorizador |

Los errores se devuelven como ProblemDetails con mensajes en español: 400 (datos inválidos, con el error de cada campo), 401 (sin sesión), 403 (sin permiso), 404 (no existe), 409 (acción no disponible en el estado actual o cambio simultáneo de otro usuario) y 500.

## Pruebas

Corre todas las pruebas (unitarias y de integración):

```bash
dotnet test
```

Solo las unitarias (no necesitan base de datos):

```bash
dotnet test tests/MPTextil.Tests
```

### Pruebas unitarias (`tests/MPTextil.Tests`)

185 pruebas con xUnit que cubren:

- La máquina de estados: transiciones válidas e inválidas, rol y pertenencia.
- Validaciones del dominio y de las peticiones.
- Fecha compromiso tras un rechazo y cálculo de vencidos.
- Historial y doble aprobación.
- Cálculo de indicadores.
- Registro de evaluación: no sube fotos sin permiso y borra las ya subidas si el guardado falla.
- Hasher de contraseñas, generador de JWT, zona horaria y firma de imágenes.

### Pruebas de integración (`tests/MPTextil.Tests.Integracion`)

Levantan la API completa en memoria con `WebApplicationFactory` y la prueban por HTTP, igual que el front: inicio de sesión, permisos por rol, consultas, el flujo completo de una solicitud con fotos, los errores en ProblemDetails, los indicadores y Swagger.

- **Necesitan SQL Server.** Cada corrida crea una base temporal `MPTextil_Integracion_<id>`, deja que la API la migre y siembre los datos de ejemplo, y la borra al terminar. Tu base `MPTextilSolutions` no se toca.
- **No necesitan Azurite.** Las fotos se guardan en un almacenamiento en memoria.
- Por defecto usan `(localdb)\MSSQLLocalDB`. Para usar otra instancia, define la variable de entorno `MPTEXTIL_PRUEBAS_SERVIDOR_SQL` antes de correrlas:

```bash
set MPTEXTIL_PRUEBAS_SERVIDOR_SQL=localhost
```

> En PowerShell: `$env:MPTEXTIL_PRUEBAS_SERVIDOR_SQL = "localhost"`. Si una corrida se interrumpe a la mitad, puede quedar una base `MPTextil_Integracion_...` que se puede borrar sin problema.

## Decisiones de diseño

- **Arquitectura por capas con patrón repository y Unit of Work.** Las interfaces viven en Application y la implementación con EF Core en Infrastructure.
- **Dominio rico.** Cada acción del flujo es un método de `Solicitud` que valida rol, pertenencia y estado antes de cambiar nada. La tabla de transiciones está en un solo lugar (`FlujoSolicitud`).
- **Doble aprobación.** Se agregó el rol **Autorizador**: el solicitante que creó la solicitud aprueba la muestra y el autorizador da el visto bueno final.
- **"Vencida" no es un estado guardado.** Se calcula: la solicitud está En progreso y la fecha compromiso vigente ya pasó.
- **Histórico de fechas compromiso.** Cada fecha queda guardada en `CompromisoEntrega`, así cambiar la fecha no borra un vencimiento. Esto permite medir con precisión el **% de entregas a tiempo** = a tiempo ÷ (a tiempo + tarde + vencidos sin envío).
- **Rechazado no es un estado de reposo.** La decisión regresa la solicitud a En progreso y queda registrada en `Decision` y en el historial.
- **Zona horaria.** El "hoy" se calcula con la zona de Honduras (`America/Tegucigalpa`), así una solicitud no se marca vencida por el desfase con UTC.
- **Concurrencia.** `rowversion` en la solicitud e índices únicos evitan que dos usuarios cambien la misma solicitud a la vez; el usuario recibe un mensaje para recargar.
- **Validación en tres capas.** Un filtro global ejecuta FluentValidation antes del controlador, el dominio vuelve a validar sus reglas y la base de datos tiene restricciones CHECK e índices únicos.
- **Fotos seguras.** El contenedor es privado y la API entrega enlaces temporales de solo lectura (SAS) de 60 minutos. Además del tipo declarado, se revisan los primeros bytes del archivo para rechazar archivos que no son imágenes.
- **Contraseñas** con PBKDF2-SHA256 (600 000 iteraciones, sal por usuario). Si el correo no existe se calcula igual un hash, para que el tiempo de respuesta no revele qué correos están registrados.
- **Mensajes para usuarios.** Todos los errores tienen título y detalle en español sin términos técnicos, y el front los muestra junto al campo correspondiente.

## Qué quedó fuera y por qué

| Pendiente | Motivo |
|---|---|
| Despliegue en la nube | Se publica por separado; la aplicación ya lee su configuración (cadena de conexión, Blob Storage, clave JWT, CORS) desde `appsettings` o variables de entorno. |
| Administración de usuarios, proveedores y catálogos | No lo pide la prueba; los datos de ejemplo cubren todos los roles. El dominio ya permite desactivar proveedores y tipos de material. |
| Reasignar después de aceptar | Una vez que el proveedor acepta no se cambia de proveedor, para no distorsionar su indicador de cumplimiento. Mientras está Asignado puede declinar. |
| Notificaciones por correo | Fuera del alcance; el historial y los indicadores muestran el avance. |
| Renovación de token | El token dura 8 horas; al vencer, el portal pide iniciar sesión de nuevo. |
| Pruebas automatizadas de la interfaz | Se cubrió la API con pruebas unitarias y de integración; el front se verificó manualmente en el navegador con cada rol. |
| Paginación en el servidor | El volumen de la prueba es pequeño; el front pagina en pantalla. |

## Problemas comunes

| Mensaje o síntoma | Solución |
|---|---|
| La API no arranca: *"No se pudo conectar al almacenamiento de archivos"* | Inicia Azurite (sección 2) antes que la API. |
| La API no arranca: error de conexión a SQL Server | Revisa la cadena de conexión `ConnectionStrings:MPTextil` y que la instancia esté encendida (`sqllocaldb start MSSQLLocalDB` para LocalDB). |
| `dotnet` indica que no encuentra el SDK | Instala el SDK de .NET 10.0.300 o superior (`global.json` lo exige). |
| El portal dice *"No pudimos conectar con el servidor"* | La API no está corriendo en `http://localhost:5236`. |
| El navegador muestra un error de CORS | Si cambiaste el puerto del front, agrégalo a `Cors:OrigenesPermitidos` en `appsettings.Development.json` de la API. |
| Quiero empezar con los datos de ejemplo limpios | Borra la base `MPTextilSolutions` (desde SQL Server Management Studio o con los comandos de abajo) y arranca la API: la vuelve a crear y sembrar. |

Borrar la base desde la terminal:

```bash
dotnet tool restore
```

```bash
dotnet ef database drop --force --project src/MPTextil.Infrastructure --startup-project src/MPTextil.Api
```

> En PowerShell ejecuta antes `$env:ASPNETCORE_ENVIRONMENT = "Development"` para que el comando lea la cadena de conexión de desarrollo.
