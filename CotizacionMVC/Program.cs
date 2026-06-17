using CotizacionMVC.Data;
using CotizacionMVC.Servicios;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// ✅ CORREGIDO: Usar IDocumento en lugar de IDocument
builder.Services.AddScoped<IDocumento, PdfCotizacion>();
// builder.Services.AddScoped<PdfCotizacion>(); // Esta línea es opcional, solo si necesitas inyectar PdfCotizacion directamente

// Registrar el DbContext con PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(opciones =>
    opciones.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ========== HABILITAR SESIONES ==========
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opciones =>
{
    opciones.IdleTimeout = TimeSpan.FromMinutes(30);
    opciones.Cookie.HttpOnly = true;
    opciones.Cookie.IsEssential = true;
});

QuestPDF.Settings.License = LicenseType.Community;

// Para acceder a la sesión desde las vistas
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ========== USAR SESIONES ==========
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();