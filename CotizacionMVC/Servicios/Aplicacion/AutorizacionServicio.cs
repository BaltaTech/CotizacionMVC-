using CotizacionMVC.Data;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
namespace CotizacionMVC.Servicios.Aplicacion
{
    public class AutorizacionServicio : IAutorizacionServicio
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ApplicationDbContext _context;

        public AutorizacionServicio(
            UserManager<Usuario> userManager,
            IHttpContextAccessor httpContextAccessor,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _context = context;
        }

        public async Task<bool> EsAdminAsync(Guid usuarioId)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return false;
            return await _userManager.IsInRoleAsync(usuario, "Administrador");
        }

        public async Task<bool> EsVendedorAsync(Guid usuarioId)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return false;
            return await _userManager.IsInRoleAsync(usuario, "Vendedor");
        }

        public async Task<bool> EsRecepcionAsync(Guid usuarioId)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return false;
            return await _userManager.IsInRoleAsync(usuario, "Recepcion");
        }

        public async Task<List<string>> ObtenerRolesAsync(Guid usuarioId)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return new List<string>();
            var roles = await _userManager.GetRolesAsync(usuario);
            return roles.ToList();
        }

        // ========== VERIFICACIONES DE ACCESO ==========

        public async Task<bool> PuedeAccederASeccionAsync(Guid usuarioId, string seccion)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return false;

            var roles = await _userManager.GetRolesAsync(usuario);

            return seccion switch
            {
                "Cotizaciones" => roles.Contains("Administrador") || roles.Contains("Vendedor"),
                "Clientes" => roles.Contains("Administrador") || roles.Contains("Vendedor"),
                "Recepcion" => roles.Contains("Administrador") || roles.Contains("Recepcion"),
                "Usuarios" => roles.Contains("Administrador"),
                "Empresas" => roles.Contains("Administrador"),
                "Equipos" => roles.Contains("Administrador"),
                "Instalaciones" => roles.Contains("Administrador") || roles.Contains("Recepcion"),
                "Dashboard" => roles.Contains("Administrador") || roles.Contains("Recepcion"),
                "Seguimiento" => roles.Contains("Administrador") || roles.Contains("Vendedor"),
                _ => false
            };
        }

        public async Task<bool> TieneAccesoAEmpresaAsync(Guid usuarioId, Guid empresaId)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return false;

            // Admin y Recepción tienen acceso a TODAS las empresas
            if (await _userManager.IsInRoleAsync(usuario, "Administrador") ||
                await _userManager.IsInRoleAsync(usuario, "Recepcion"))
                return true;

            // Vendedor: verificar si tiene acceso a esta empresa
            if (await _userManager.IsInRoleAsync(usuario, "Vendedor"))
            {
                // 1. Verificar si está en EmpresasAcceso
                var usuarioConEmpresas = await _userManager.Users
                    .Include(u => u.EmpresasAcceso)
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);

                if (usuarioConEmpresas?.EmpresasAcceso != null &&
                    usuarioConEmpresas.EmpresasAcceso.Any(e => e.Id == empresaId))
                    return true;

                // 2. Verificar si tiene actividad en esta empresa (leads o cotizaciones)
                var tieneActividad = await _context.Leads
                    .AnyAsync(l => l.VendedorAsignadoId == usuarioId && l.EmpresaId == empresaId)
                    || await _context.Cotizaciones
                    .AnyAsync(c => c.VendedorId == usuarioId && c.EmpresaId == empresaId);

                return tieneActividad;
            }

            return false;
        }

        public async Task<Guid?> ObtenerEmpresaActivaIdAsync(Guid usuarioId)
        {
            // 1. Intentar obtener de la sesión
            var empresaIdString = _httpContextAccessor.HttpContext?.Session.GetString("EmpresaActivaId");
            if (!string.IsNullOrEmpty(empresaIdString) && Guid.TryParse(empresaIdString, out var empresaId))
            {
                if (await TieneAccesoAEmpresaAsync(usuarioId, empresaId))
                    return empresaId;
            }

            // 2. Si no hay sesión o no tiene acceso, obtener según el rol
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return null;

            // Admin y Recepción: primera empres    a activa
            if (await _userManager.IsInRoleAsync(usuario, "Administrador") ||
                await _userManager.IsInRoleAsync(usuario, "Recepcion"))
            {
                var primeraEmpresa = await _context.Empresas
                    .Where(e => e.Activa)
                    .OrderBy(e => e.FechaCreacion)
                    .FirstOrDefaultAsync();
                return primeraEmpresa?.Id;
            }

            // Vendedor: empresa donde tiene actividad
            if (await _userManager.IsInRoleAsync(usuario, "Vendedor"))
            {
                // Buscar en leads asignados
                var lead = await _context.Leads
                    .Where(l => l.VendedorAsignadoId == usuarioId)
                    .OrderByDescending(l => l.FechaAsignacion)
                    .FirstOrDefaultAsync();

                if (lead != null && lead.EmpresaId != Guid.Empty)
                    return lead.EmpresaId;

                // Buscar en cotizaciones
                var cotizacion = await _context.Cotizaciones
                    .Where(c => c.VendedorId == usuarioId)
                    .OrderByDescending(c => c.FechaCreacion)
                    .FirstOrDefaultAsync();

                if (cotizacion != null && cotizacion.EmpresaId != Guid.Empty)
                    return cotizacion.EmpresaId;

                // Buscar en EmpresasAcceso
                var usuarioConEmpresas = await _userManager.Users
                    .Include(u => u.EmpresasAcceso)
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);

                return usuarioConEmpresas?.EmpresasAcceso.FirstOrDefault()?.Id;
            }

            return null;
        }

        public async Task<Empresa?> ObtenerEmpresaActivaAsync(Guid usuarioId)
        {
            var empresaId = await ObtenerEmpresaActivaIdAsync(usuarioId);
            if (!empresaId.HasValue) return null;

            return await _context.Empresas.FindAsync(empresaId.Value);
        }

        public async Task<IQueryable<Cotizacion>> FiltrarCotizacionesAsync(Guid usuarioId, IQueryable<Cotizacion> query)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return query.Where(c => false);

            var roles = await _userManager.GetRolesAsync(usuario);
            var empresaId = await ObtenerEmpresaActivaIdAsync(usuarioId);

            // Filtrar por empresa activa
            if (empresaId.HasValue)
                query = query.Where(c => c.EmpresaId == empresaId.Value);

            // Admin ve todas las cotizaciones de la empresa
            if (roles.Contains("Administrador"))
                return query;

            // Vendedor ve solo sus cotizaciones
            if (roles.Contains("Vendedor"))
                return query.Where(c => c.VendedorId == usuarioId);

            // Recepción no ve cotizaciones
            return query.Where(c => false);
        }

        public async Task<IQueryable<Lead>> FiltrarLeadsAsync(Guid usuarioId, IQueryable<Lead> query)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return query.Where(l => false);

            var roles = await _userManager.GetRolesAsync(usuario);
            var empresaId = await ObtenerEmpresaActivaIdAsync(usuarioId);

            // Filtrar por empresa activa
            if (empresaId.HasValue)
                query = query.Where(l => l.EmpresaId == empresaId.Value);

            // Admin ve todos los leads de la empresa
            if (roles.Contains("Administrador"))
                return query;

            // Vendedor ve solo sus leads asignados
            if (roles.Contains("Vendedor"))
                return query.Where(l => l.VendedorAsignadoId == usuarioId);

            // Recepción ve leads sin vendedor asignado
            if (roles.Contains("Recepcion"))
                return query.Where(l => l.VendedorAsignadoId == null);

            return query.Where(l => false);
        }

        public async Task<IQueryable<Cliente>> FiltrarClientesAsync(Guid usuarioId, IQueryable<Cliente> query)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return query.Where(c => false);

            var roles = await _userManager.GetRolesAsync(usuario);
            var empresaId = await ObtenerEmpresaActivaIdAsync(usuarioId);

            // Para filtrar clientes por empresa, pasamos por sus cotizaciones y leads
            if (empresaId.HasValue)
            {
                query = query.Where(c => c.Cotizaciones.Any(co => co.EmpresaId == empresaId.Value)
                    || _context.Leads.Any(l => l.ClienteId == c.Id && l.EmpresaId == empresaId.Value));
            }

            // Admin ve todos los clientes de la empresa
            if (roles.Contains("Administrador"))
                return query;

            // Vendedor ve clientes de sus cotizaciones o leads
            if (roles.Contains("Vendedor"))
            {
                return query.Where(c => c.Cotizaciones.Any(co => co.VendedorId == usuarioId)
                    || _context.Leads.Any(l => l.ClienteId == c.Id && l.VendedorAsignadoId == usuarioId));
            }

            // Recepción ve todos los clientes de la empresa
            if (roles.Contains("Recepcion"))
                return query;

            return query.Where(c => false);
        }
    }
}