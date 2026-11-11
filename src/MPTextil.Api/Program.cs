using MPTextil.Api.Configuracion;
using MPTextil.Api.Documentacion;
using MPTextil.Api.Errores;
using MPTextil.Api.Seguridad;
using MPTextil.Application;
using MPTextil.Infrastructure;
using MPTextil.Infrastructure.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AgregarAplicacion();
builder.Services.AgregarInfraestructura(builder.Configuration);
builder.Services.AgregarControladores();
builder.Services.AgregarManejoErrores();
builder.Services.AgregarSeguridad();
builder.Services.AgregarDocumentacionApi();
builder.Services.AgregarCors(builder.Configuration);

var app = builder.Build();

var datosEjemplo = app.Configuration.GetSection(OpcionesDatosEjemplo.Seccion).Get<OpcionesDatosEjemplo>()
    ?? new OpcionesDatosEjemplo();
await app.Services.InicializarDatosAsync(datosEjemplo);

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.UsarDocumentacionApi();
app.UsarCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
