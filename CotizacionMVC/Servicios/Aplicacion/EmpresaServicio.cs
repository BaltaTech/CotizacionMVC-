using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Empresa;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class EmpresaServicio : IEmpresaServicio
    {
        private readonly IEmpresaRepository _empresaRepository;

        public EmpresaServicio(IEmpresaRepository empresaRepository)
        {
            _empresaRepository = empresaRepository;
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
