using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class CotizacionServicio
    {
        private readonly ICotizacionRepository _cotizacionRepo;
        private readonly IClienteRepository _clienteRepo;
        private readonly IEquipoRepository _equipoRepo;
        private readonly IInstalacionRepository _instalacionRepo;
        private readonly IEmpresaRepository _empresaRepo;

        public CotizacionServicio(
            ICotizacionRepository cotizacionRepo,
            IClienteRepository clienteRepo,
            IEquipoRepository equipoRepo,
            IInstalacionRepository instalacionRepo,
            IEmpresaRepository empresaRepo)
        {
            _cotizacionRepo = cotizacionRepo;
            _clienteRepo = clienteRepo;
            _equipoRepo = equipoRepo;
            _instalacionRepo = instalacionRepo;
            _empresaRepo = empresaRepo;
        }

        public async Task<ResultadoCrearCotizacion> CrearCotizacionAsync(SolicitudCrearCotizacion solicitud)
        {
            // PASO 1: VALIDAR DATOS DE ENTRADA
            if (solicitud.ClienteId == Guid.Empty)
                return ResultadoCrearCotizacion.Error("Debe seleccionar un cliente");

            if (solicitud.Equipos == null || !solicitud.Equipos.Any())
                return ResultadoCrearCotizacion.Error("Debe agregar al menos un equipo");

            if (solicitud.AreaMetrosCuadrados <= 0)
                return ResultadoCrearCotizacion.Error("El área debe ser mayor a cero");

            // PASO 2: BUSCAR ENTIDADES
            var cliente = await _clienteRepo.GetByIdAsync(solicitud.ClienteId);
            if (cliente == null)
                return ResultadoCrearCotizacion.Error("Cliente no encontrado");

            var empresa = await _empresaRepo.GetByIdAsync(solicitud.EmpresaId);
            if (empresa == null)
                return ResultadoCrearCotizacion.Error("Empresa no encontrada");

            // PASO 3: VALIDAR REGLAS DE DOMINIO DEL CLIENTE
            if (!cliente.TieneContacto())
                return ResultadoCrearCotizacion.Error("El cliente no tiene información de contacto.");

            if (!cliente.TieneDireccion())
                return ResultadoCrearCotizacion.Error("El cliente no tiene dirección registrada.");

            // PASO 4: VALIDAR EQUIPOS ANTES DE CREAR COTIZACIÓN
            foreach (var eq in solicitud.Equipos)
            {
                var equipo = await _equipoRepo.GetByIdAsync(eq.EquipoId);
                if (equipo == null)
                    return ResultadoCrearCotizacion.Error($"Equipo con ID {eq.EquipoId} no encontrado");

                // Validar regla Trane ANTES de crear la cotización
                if (empresa.EsExclusivaTrane && !equipo.EsMarcaTrane())
                    return ResultadoCrearCotizacion.Error(
                        $"Esta empresa solo puede cotizar equipos Trane. El equipo {equipo.Marca} {equipo.Modelo} no está permitido.");

                if (!equipo.Activo)
                    return ResultadoCrearCotizacion.Error($"El equipo {equipo.Modelo} no está disponible actualmente.");
            }

            // PASO 5: GENERAR NÚMERO Y CREAR COTIZACIÓN
            var numeroCotizacion = await _cotizacionRepo.GenerarSiguienteNumeroAsync();

            Cotizacion cotizacion;
            try
            {
                cotizacion = new Cotizacion(numeroCotizacion, cliente, empresa, solicitud.Vendedor,
                    solicitud.AreaMetrosCuadrados, solicitud.CondicionesPago ?? "");
            }
            catch (ArgumentException ex)
            {
                return ResultadoCrearCotizacion.Error(ex.Message);
            }

            // PASO 6: AGREGAR EQUIPOS (ya validados, no deberían fallar)
            foreach (var eq in solicitud.Equipos)
            {
                var equipo = await _equipoRepo.GetByIdAsync(eq.EquipoId);
                cotizacion.AgregarEquipo(equipo!, eq.Cantidad, empresa.UtilidadEmpresaPorcentaje,
                    empresa.UtilidadVendedorPorcentaje, null);
            }

            // PASO 7: AGREGAR INSTALACIONES
            foreach (var inst in solicitud.Instalaciones)
            {
                if (inst.InstalacionId.HasValue)
                {
                    var instalacion = await _instalacionRepo.GetByIdAsync(inst.InstalacionId.Value);
                    if (instalacion == null || !instalacion.Activo) continue;
                    cotizacion.AgregarInstalacionPredefinida(instalacion, inst.Cantidad);
                }
                else
                {
                    cotizacion.AgregarInstalacion(inst.Concepto, inst.Descripcion ?? "", inst.Cantidad, inst.CostoUnitario);
                }
            }

            // PASO 8: GUARDAR
            await _cotizacionRepo.AddAsync(cotizacion);
            return ResultadoCrearCotizacion.Exito(cotizacion);
        }
    }
}