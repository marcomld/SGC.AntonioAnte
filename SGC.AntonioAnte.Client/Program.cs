using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SGC.AntonioAnte.Client;
using SGC.AntonioAnte.Client.Infrastructure.Http;
using SGC.AntonioAnte.Client.Infrastructure.Security;
using SGC.AntonioAnte.Client.Services;
using SGC.AntonioAnte.Client.Services.Catastro.Contracts;
using SGC.AntonioAnte.Client.Services.Catastro.Implementation;
using SGC.AntonioAnte.Client.Services.Contracts;
using SGC.AntonioAnte.Client.Services.Seguridad.Contracts;
using SGC.AntonioAnte.Client.Services.Seguridad.Implementation;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiUrlBase = "https://localhost:7245";

// 1. Registros Core de Seguridad y Utilidades
builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// Registro del Servicio de Usuarios (NUEVO)
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IDepartamentoService, DepartamentoService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();

// Registro de servicios del Módulo 2: Catastro
builder.Services.AddScoped<ICatalogoService, CatalogoService>();
builder.Services.AddScoped<IPropietarioService, PropietarioService>();
builder.Services.AddScoped<IPredioService, PredioService>();

// 2. Registro del Interceptor
builder.Services.AddTransient<IdentityHandler>();

// 3. Configuración del HttpClient Principal (Con Interceptor)
builder.Services.AddHttpClient("SGC.API", client =>
{
    client.BaseAddress = new Uri(apiUrlBase);
})
.AddHttpMessageHandler<IdentityHandler>();

// Hacemos que por defecto cualquier inyección de HttpClient use el que tiene el Interceptor
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("SGC.API"));

// 4. Configuración del HttpClient de Autenticación (Sin Interceptor para evitar bucles en RefreshToken)
builder.Services.AddHttpClient("AuthClient", client =>
{
    client.BaseAddress = new Uri(apiUrlBase);
});

await builder.Build().RunAsync();
