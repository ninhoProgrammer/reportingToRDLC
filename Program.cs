using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Reporteadores.Models; // Ajusta el espacio de nombres

var builder = WebApplication.CreateBuilder(args);

// Configuraci�n de la cadena de conexi�n
builder.Services.AddDbContext<ComparteContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configuraci�n de Identity
builder.Services.AddIdentity<Usuario, IdentityRole>()
    .AddEntityFrameworkStores<ComparteContext>()
    .AddDefaultTokenProviders();

// Otros servicios
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(5); // Tiempo de inactividad de 5 minutos
    options.Cookie.HttpOnly = true; // Aumenta la seguridad marcando la cookie como HttpOnly
    options.Cookie.IsEssential = true; // Asegura que la cookie de sesi�n no sea eliminada por GDPR
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

var app = builder.Build();

// Configuraci�n de middlewares
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

app.Run();
