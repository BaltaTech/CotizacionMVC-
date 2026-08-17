using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Empresa;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class EmpresaServicio : IEmpresaServicio
    {
        private readonly IEmpresaRepository _empresaRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAutorizacionServicio _autorizacionServicio;

        public EmpresaServicio(
            IEmpresaRepository empresaRepository,
            IHttpContextAccessor httpContextAccessor,
            IAutorizacionServicio autorizacionServicio)
        {
            _empresaRepository = empresaRepository;
            _httpContextAccessor = httpContextAccessor;
            _autorizacionServicio = autorizacionServicio;
        }

        public async Task<IReadOnlyList<EmpresaResumenDto>> ObtenerTodasAsync()
        {
            var query = _empresaRepository.ObtenerQueryable();

            return await query
                .OrderBy(e => e.NombreComercial)
                .Select(e => new EmpresaResumenDto
                {
                    Id = e.Id,
                    NombreComercial = e.NombreComercial,
                    Slug = e.Slug,
                    Activa = e.Activa,
                    LogoUrl = e.LogoUrl,
                    ColorPrimario = e.ColorPrimario
                })
                .ToListAsync();
        }

        public async Task<EmpresaDetalleDto?> ObtenerPorIdAsync(Guid id)
        {
            var empresa = await _empresaRepository.GetByIdAsync(id);

            if (empresa == null)
                return null;

            return MapearADetalleDto(empresa);
        }

        public async Task<EmpresaDetalleDto> ActualizarAsync(ActualizarEmpresaDto dto)
        {
            var empresa = await _empresaRepository.GetByIdAsync(dto.Id)
                ?? throw new KeyNotFoundException($"No se encontró la empresa con ID {dto.Id}");

            empresa.ActualizarUtilidades(dto.UtilidadEmpresaPorcentaje, dto.UtilidadVendedorPorcentaje);
            empresa.ActualizarContacto(dto.TelefonoContacto, dto.CorreoContacto);
            empresa.ConfigurarIdentidadVisual(dto.LogoUrl, dto.ColorPrimario, dto.ColorSecundario, dto.PlantillaPdfNombre, dto.Eslogan);

            _empresaRepository.Update(empresa);
            await _empresaRepository.SaveChangesAsync();

            return MapearADetalleDto(empresa);
        }

        public async Task<EmpresaDetalleDto?> ObtenerEmpresaActualAsync()
        {
            var query = _empresaRepository.ObtenerQueryable();

            var empresa = await query.FirstOrDefaultAsync(e => e.Activa);

            if (empresa == null)
                return null;

            return MapearADetalleDto(empresa);
        }

        // ✅ NUEVO: Validar acceso a empresa
        public async Task<bool> TieneAccesoAEmpresaAsync(Guid usuarioId, Guid empresaId)
        {
            return await _autorizacionServicio.TieneAccesoAEmpresaAsync(usuarioId, empresaId);
        }

        // ✅ NUEVO: Establecer empresa activa en sesión
        public async Task EstablecerEmpresaActivaAsync(Guid empresaId)
        {
            var empresa = await _empresaRepository.GetByIdAsync(empresaId);
            if (empresa == null)
                throw new KeyNotFoundException($"No se encontró la empresa con ID {empresaId}");

            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null)
                throw new InvalidOperationException("No se puede acceder a la sesión");

            session.SetString("EmpresaActivaId", empresa.Id.ToString());
            session.SetString("EmpresaActivaNombre", empresa.NombreComercial);
            session.SetString("EmpresaActivaSlug", empresa.Slug);
            session.SetString("EmpresaEsExclusivaTrane", empresa.EsExclusivaTrane.ToString());
            session.SetString("EmpresaColorPrimario", empresa.ColorPrimario ?? "#C8102E");
            session.SetString("EmpresaColorSecundario", empresa.ColorSecundario ?? "#FFFFFF");
        }

        private EmpresaDetalleDto MapearADetalleDto(Empresa empresa)
        {
            return new EmpresaDetalleDto
            {
                Id = empresa.Id,
                NombreComercial = empresa.NombreComercial,
                NombreLegal = empresa.NombreLegal,
                Slug = empresa.Slug,
                EsExclusivaTrane = empresa.EsExclusivaTrane,
                MonedaBase = empresa.MonedaBase,
                UtilidadEmpresaPorcentaje = empresa.UtilidadEmpresaPorcentaje,
                UtilidadVendedorPorcentaje = empresa.UtilidadVendedorPorcentaje,
                LogoUrl = empresa.LogoUrl,
                ColorPrimario = empresa.ColorPrimario,
                ColorSecundario = empresa.ColorSecundario,
                PlantillaPdfNombre = empresa.PlantillaPdfNombre,
                TelefonoContacto = empresa.TelefonoContacto,
                CorreoContacto = empresa.CorreoContacto,
                SitioWeb = empresa.SitioWeb,
                Eslogan = empresa.Eslogan,
                Activa = empresa.Activa,
                FechaCreacion = empresa.FechaCreacion
            };
        }
    }
}