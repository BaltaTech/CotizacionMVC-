using Microsoft.EntityFrameworkCore;
using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Equipo;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class EquipoServicio : IEquipoServicio
    {
        private readonly IEquipoRepository _equipoRepository;

        public EquipoServicio(IEquipoRepository equipoRepository)
        {
            _equipoRepository = equipoRepository;
        }

        public async Task<IReadOnlyList<EquipoResumenDto>> ObtenerTodosAsync()
        {
            var query = _equipoRepository.ObtenerQueryable()
                .Where(e => e.Activo);

            return await query
                .OrderBy(e => e.Marca)
                .ThenBy(e => e.Modelo)
                .Select(e => new EquipoResumenDto
                {
                    Id = e.Id,
                    Marca = e.Marca,
                    Modelo = e.Modelo,
                    CapacidadToneladas = e.CapacidadToneladas,
                    PrecioBase = e.PrecioBase,
                    MonedaOriginal = e.MonedaOriginal,
                    Activo = e.Activo
                })
                .ToListAsync();
        }

        public async Task<EquipoDetalleDto?> ObtenerPorIdAsync(Guid id)
        {
            var equipo = await _equipoRepository.GetByIdAsync(id);

            if (equipo == null)
                return null;

            return MapearADetalleDto(equipo);
        }

        public async Task<EquipoDetalleDto> CrearAsync(CrearEquipoDto dto)
        {
            var equipo = new Equipo(
                dto.Marca,
                dto.Modelo,
                dto.CapacidadToneladas,
                dto.PrecioBase,
                dto.MonedaOriginal
            );

            if (!string.IsNullOrWhiteSpace(dto.Tipo))
                equipo.CompletarDetalles(dto.Tipo, dto.Tension ?? "", dto.Tecnologia ?? "");

            await _equipoRepository.AddAsync(equipo);
            await _equipoRepository.SaveChangesAsync();

            return MapearADetalleDto(equipo);
        }

        public async Task<EquipoDetalleDto> ActualizarAsync(ActualizarEquipoDto dto)
        {
            var equipo = await _equipoRepository.GetByIdAsync(dto.Id)
                ?? throw new KeyNotFoundException($"No se encontró el equipo con ID {dto.Id}");

            equipo.ActualizarPrecio(dto.PrecioBase);

            _equipoRepository.Update(equipo);
            await _equipoRepository.SaveChangesAsync();

            return MapearADetalleDto(equipo);
        }

        public async Task<EliminarEquipoResultado> EliminarAsync(Guid id)
        {
            var equipo = await _equipoRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException($"No se encontró el equipo con ID {id}");

            var estaEnUso = await _equipoRepository.EstaEnUsoAsync(id);

            if (estaEnUso)
            {
                equipo.Desactivar();
                _equipoRepository.Update(equipo);
                await _equipoRepository.SaveChangesAsync();

                return new EliminarEquipoResultado
                {
                    Eliminado = false,
                    Desactivado = true,
                    Mensaje = $"El equipo {equipo.Modelo} está en uso en cotizaciones. Se ha desactivado pero no eliminado."
                };
            }

            _equipoRepository.Delete(equipo);
            await _equipoRepository.SaveChangesAsync();

            return new EliminarEquipoResultado
            {
                Eliminado = true,
                Desactivado = false,
                Mensaje = $"Equipo {equipo.Modelo} eliminado exitosamente"
            };
        }

        private EquipoDetalleDto MapearADetalleDto(Equipo equipo)
        {
            return new EquipoDetalleDto
            {
                Id = equipo.Id,
                Marca = equipo.Marca,
                Modelo = equipo.Modelo,
                Tipo = equipo.Tipo,
                CapacidadToneladas = equipo.CapacidadToneladas,
                Tension = equipo.Tension,
                Tecnologia = equipo.Tecnologia,
                PrecioBase = equipo.PrecioBase,
                MonedaOriginal = equipo.MonedaOriginal,
                Activo = equipo.Activo,
                FechaCreacion = equipo.FechaCreacion
            };
        }
    }
}
