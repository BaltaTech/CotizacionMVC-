using CotizacionMVC.Data.Importadores;
using CotizacionMVC.Data;
using CotizacionMVC.Data.CargaDatos;
using CotizacionMVC.Data.Repositorios.Implementaciones;
using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Hubs;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios;
using CotizacionMVC.Servicios.Aplicacion;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.Servicios.Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ========== Protección global: todo requiere autenticación ==========
builder.Services.AddControllersWithViews(config =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    config.Filters.Add(new AuthorizeFilter(policy));
});

// ========== SignalR ==========
builder.Services.AddSignalR();

// ========== Servicios de infraestructura ==========
builder.Services.AddScoped<NotificacionServicio>();
builder.Services.AddHostedService<RecordatorioBackgroundService>();

// ========== Servicios de aplicación ==========
builder.Services.AddScoped<IDocumento, PdfCotizacion>();
builder.Services.AddScoped<IClienteServicio, ClienteServicio>();
builder.Services.AddScoped<IEmpresaServicio, EmpresaServicio>();
builder.Services.AddScoped<ICotizacionServicio, CotizacionServicio>();
builder.Services.AddScoped<IEquipoServicio, EquipoServicio>();
builder.Services.AddScoped<IRecepcionServicio, RecepcionServicio>();
builder.Services.AddScoped<ISeguimientoServicio, SeguimientoServicio>();
builder.Services.AddScoped<IInstalacionServicio, InstalacionServicio>();

// ========== Repositorios ==========
builder.Services.AddScoped<ICotizacionRepository, CotizacionRepository>();
builder.Services.AddScoped<IClienteRepository, ClienteRepository>();
builder.Services.AddScoped<IEquipoRepository, EquipoRepository>();
builder.Services.AddScoped<IInstalacionRepository, InstalacionRepository>();
builder.Services.AddScoped<IEmpresaRepository, EmpresaRepository>();
builder.Services.AddScoped<ISeguimientoRepository, SeguimientoRepository>();

// Registrar el DbContext con PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========== Configuración de Identity ==========
builder.Services.AddIdentity<Usuario, IdentityRole<Guid>>(opciones =>
{
    opciones.Password.RequiredLength = 6;
    opciones.Password.RequireNonAlphanumeric = false;
    opciones.Password.RequireDigit = false;
    opciones.Password.RequireLowercase = false;
    opciones.Password.RequireUppercase = false;
    opciones.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configurar la cookie de autenticación
builder.Services.ConfigureApplicationCookie(opciones =>
{
    opciones.LoginPath = "/Autenticacion/Login";
    opciones.AccessDeniedPath = "/Autenticacion/AccesoDenegado";
    opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
    opciones.SlidingExpiration = true;
});

// ========== Mantenemos las sesiones para el selector de empresa ==========
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    opciones.IdleTimeout = TimeSpan.FromMinutes(30);
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
});

QuestPDF.Settings.License = LicenseType.Community;

// Para acceder a la sesión y al contexto HTTP desde las vistas
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ========== Cargar datos iniciales e importaciones ==========
using (var scope = app.Services.CreateScope())
{
    var servicios = scope.ServiceProvider;
    await CargadorDatosIniciales.CargarAsync(servicios);

    var context = servicios.GetRequiredService<ApplicationDbContext>();

    // Importar instalaciones (solo primera vez)
    if (!await context.Instalaciones.AnyAsync())
    {
        var rutaCsvServicios = @"C:\Users\Airey\source\repos\CotizacionMVC\CotizacionMVC\servicios.csv";
        if (File.Exists(rutaCsvServicios))
        {
            await ImportadorInstalaciones.ImportarDesdeCsvAsync(context, rutaCsvServicios);
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Autenticacion}/{action=Login}/{id?}");

app.MapHub<NotificacionHub>("/notificacionHub");

app.Run();