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

            if (await _userManager.IsInRoleAsync(usuario, "Administrador") ||
                await _userManager.IsInRoleAsync(usuario, "Recepcion"))
                return true;

            if (await _userManager.IsInRoleAsync(usuario, "Vendedor"))
            {
                var usuarioConEmpresas = await _userManager.Users
                    .Include(u => u.EmpresasAcceso)
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);

                if (usuarioConEmpresas?.EmpresasAcceso != null &&
                    usuarioConEmpresas.EmpresasAcceso.Any(e => e.Id == empresaId))
                    return true;

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

            // 2. Si no hay sesión, obtener según el rol
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return null;

            // Admin y Recepción: primera empresa activa
            if (await _userManager.IsInRoleAsync(usuario, "Administrador") ||
                await _userManager.IsInRoleAsync(usuario, "Recepcion"))
            {
                var primeraEmpresa = await _context.Empresas
                    .Where(e => e.Activa)
                    .OrderBy(e => e.FechaCreacion)
                    .FirstOrDefaultAsync();
                return primeraEmpresa?.Id;
            }

            // Vendedor: buscar empresa por actividad
            if (await _userManager.IsInRoleAsync(usuario, "Vendedor"))
            {
                // 1. Leads asignados
                var lead = await _context.Leads
                    .Where(l => l.VendedorAsignadoId == usuarioId)
                    .OrderByDescending(l => l.FechaAsignacion)
                    .FirstOrDefaultAsync();
                if (lead != null && lead.EmpresaId != Guid.Empty)
                    return lead.EmpresaId;

                // 2. Cotizaciones
                var cotizacion = await _context.Cotizaciones
                    .Where(c => c.VendedorId == usuarioId)
                    .OrderByDescending(c => c.FechaCreacion)
                    .FirstOrDefaultAsync();
                if (cotizacion != null && cotizacion.EmpresaId != Guid.Empty)
                    return cotizacion.EmpresaId;

                // 3. EmpresasAcceso
                var usuarioConEmpresas = await _userManager.Users
                    .Include(u => u.EmpresasAcceso)
                    .FirstOrDefaultAsync(u => u.Id == usuarioId);
                if (usuarioConEmpresas?.EmpresasAcceso?.Any() == true)
                    return usuarioConEmpresas.EmpresasAcceso.First().Id;

                // 4. Fallback: primera empresa activa
                var primeraEmpresa = await _context.Empresas
                    .Where(e => e.Activa)
                    .OrderBy(e => e.FechaCreacion)
                    .FirstOrDefaultAsync();
                return primeraEmpresa?.Id;
            }

            return null;
        }

        public async Task<Empresa?> ObtenerEmpresaActivaAsync(Guid usuarioId)
        {
            var empresaId = await ObtenerEmpresaActivaIdAsync(usuarioId);
            if (!empresaId.HasValue) return null;
            return await _context.Empresas.FindAsync(empresaId.Value);
        }

        // =====================================================
        // FILTROS - Solo filtran por empresa si hay una activa
        // =====================================================

        public async Task<IQueryable<Cotizacion>> FiltrarCotizacionesAsync(Guid usuarioId, IQueryable<Cotizacion> query)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return query.Where(c => false);

            var roles = await _userManager.GetRolesAsync(usuario);
            var empresaId = await ObtenerEmpresaActivaIdAsync(usuarioId);

            // Solo filtrar por empresa si hay una seleccionada
            if (empresaId.HasValue)
                query = query.Where(c => c.EmpresaId == empresaId.Value);

            if (roles.Contains("Administrador"))
                return query;

            if (roles.Contains("Vendedor"))
                return query.Where(c => c.VendedorId == usuarioId);

            return query.Where(c => false);
        }

        public async Task<IQueryable<Lead>> FiltrarLeadsAsync(Guid usuarioId, IQueryable<Lead> query)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null) return query.Where(l => false);

            var roles = await _userManager.GetRolesAsync(usuario);
            var empresaId = await ObtenerEmpresaActivaIdAsync(usuarioId);

            // Solo filtrar por empresa si hay una seleccionada
            if (empresaId.HasValue)
                query = query.Where(l => l.EmpresaId == empresaId.Value);

            if (roles.Contains("Administrador"))
                return query;

            if (roles.Contains("Vendedor"))
                return query.Where(l => l.VendedorAsignadoId == usuarioId);

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

            // Solo filtrar por empresa si hay una seleccionada
            if (empresaId.HasValue)
            {
                query = query.Where(c => c.Cotizaciones.Any(co => co.EmpresaId == empresaId.Value)
                    || _context.Leads.Any(l => l.ClienteId == c.Id && l.EmpresaId == empresaId.Value));
            }

            if (roles.Contains("Administrador"))
                return query;

            if (roles.Contains("Vendedor"))
            {
                return query.Where(c => c.Cotizaciones.Any(co => co.VendedorId == usuarioId)
                    || _context.Leads.Any(l => l.ClienteId == c.Id && l.VendedorAsignadoId == usuarioId));
            }

            if (roles.Contains("Recepcion"))
                return query;

            return query.Where(c => false);
        }
    }
}