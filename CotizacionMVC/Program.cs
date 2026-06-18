using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios;
using CotizacionMVC.Data;
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

builder.Services.AddScoped<IDocumento, PdfCotizacion>();

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

// ========== Sembrar roles y usuario administrador inicial ==========
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAndAdminAsync(services);
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

// ==========  primero autenticación, luego autorización ==========
app.UseAuthentication();
app.UseAuthorization();

app.UseSession(); // Para el selector de empresa

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Autenticacion}/{action=Login}/{id?}");

app.Run();


// ========== Método local para inicializar roles y administrador ==========
async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
{
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    var userManager = serviceProvider.GetRequiredService<UserManager<Usuario>>();

    // Crear roles si no existen
    string[] roles = { "Administrador", "Vendedor" };
    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
            await roleManager.CreateAsync(new IdentityRole<Guid>(rol));
    }

    // Crear un usuario administrador por defecto (solo si no existe)
    var adminEmail = "admin@empresa.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var admin = new Usuario("Administrador Principal", adminEmail);
        var result = await userManager.CreateAsync(admin, "Admin123!"); // contraseña inicial (cámbiala después)
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "Administrador");
        }
    }
}