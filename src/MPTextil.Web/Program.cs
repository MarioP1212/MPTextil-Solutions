using System.Globalization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MPTextil.Web;
using MPTextil.Web.Servicios;
using MudBlazor;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var urlApi = builder.Configuration["ApiUrl"]
    ?? throw new InvalidOperationException("Configura 'ApiUrl' en wwwroot/appsettings.json con la dirección de la API.");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(urlApi) });
builder.Services.AddMudServices(opciones =>
{
    opciones.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopRight;
    opciones.SnackbarConfiguration.VisibleStateDuration = 6000;
    opciones.SnackbarConfiguration.PreventDuplicates = true;
});

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<SesionActual>();
builder.Services.AddScoped<AuthenticationStateProvider, ProveedorAutenticacion>();
builder.Services.AddScoped<ClienteApi>();
builder.Services.AddScoped<ApiAutenticacion>();
builder.Services.AddScoped<ApiCatalogos>();
builder.Services.AddScoped<ApiIndicadores>();
builder.Services.AddScoped<ApiSolicitudes>();

var cultura = CultureInfo.GetCultureInfo("es-HN");
CultureInfo.DefaultThreadCurrentCulture = cultura;
CultureInfo.DefaultThreadCurrentUICulture = cultura;

var aplicacion = builder.Build();
await aplicacion.Services.GetRequiredService<SesionActual>().CargarAsync();
await aplicacion.RunAsync();
