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
            // ==========================================
            // PASO 1: VALIDAR DATOS DE ENTRADA
            // ==========================================
            if (solicitud.ClienteId == Guid.Empty)
                return ResultadoCrearCotizacion.Error("Debe seleccionar un cliente");

            if (solicitud.Equipos == null || !solicitud.Equipos.Any())
                return ResultadoCrearCotizacion.Error("Debe agregar al menos un equipo");

            if (solicitud.AreaMetrosCuadrados <= 0)
                return ResultadoCrearCotizacion.Error("El área debe ser mayor a cero");

            // ==========================================
            // PASO 2: BUSCAR ENTIDADES
            // ==========================================
            var cliente = await _clienteRepo.GetByIdAsync(solicitud.ClienteId);
            if (cliente == null)
                return ResultadoCrearCotizacion.Error("Cliente no encontrado");

            var empresa = await _empresaRepo.GetByIdAsync(solicitud.EmpresaId);
            if (empresa == null)
                return ResultadoCrearCotizacion.Error("Empresa no encontrada");

            // ==========================================
            // PASO 3: VALIDAR REGLAS DE DOMINIO DEL CLIENTE
            // ==========================================
            if (!cliente.TieneContacto())
                return ResultadoCrearCotizacion.Error(
                    "El cliente no tiene información de contacto. Debe registrar teléfono o correo.");

            if (!cliente.TieneDireccion())
                return ResultadoCrearCotizacion.Error(
                    "El cliente no tiene dirección registrada. Debe completar la dirección antes de crear una cotización.");

            // ==========================================
            // PASO 4: GENERAR NÚMERO DE COTIZACIÓN
            // ==========================================
            var numeroCotizacion = await _cotizacionRepo.GenerarSiguienteNumeroAsync();

            // ==========================================
            // PASO 5: CREAR COTIZACIÓN (El constructor ejecuta reglas de dominio)
            // ==========================================
            Cotizacion cotizacion;
            try
            {
                cotizacion = new Cotizacion(
                    numeroCotizacion,
                    cliente,
                    empresa,
                    solicitud.Vendedor,
                    solicitud.AreaMetrosCuadrados,
                    solicitud.CondicionesPago ?? ""
                );
            }
            catch (ArgumentException ex)
            {
                return ResultadoCrearCotizacion.Error(ex.Message);
            }

            // ==========================================
            // PASO 6: AGREGAR EQUIPOS (Ejecuta reglas Trane, precios, etc.)
            // ==========================================
            foreach (var eq in solicitud.Equipos)
            {
                var equipo = await _equipoRepo.GetByIdAsync(eq.EquipoId);
                if (equipo == null)
                    return ResultadoCrearCotizacion.Error($"Equipo con ID {eq.EquipoId} no encontrado");

                try
                {                 
                    cotizacion.AgregarEquipo(
                        equipo,
                        eq.Cantidad,
                        empresa.UtilidadEmpresaPorcentaje,
                        empresa.UtilidadVendedorPorcentaje,
                        null
                    );
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("Trane"))
                {
                    return ResultadoCrearCotizacion.Error(
                        $"Error con el equipo {equipo.Modelo}: {ex.Message}");
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("activo"))
                {
                    return ResultadoCrearCotizacion.Error(
                        $"El equipo {equipo.Modelo} no está disponible actualmente.");
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    return ResultadoCrearCotizacion.Error(ex.Message);
                }
            }

            // ==========================================
            // PASO 7: AGREGAR INSTALACIONES
            // ==========================================
            foreach (var inst in solicitud.Instalaciones)
            {
                try
                {
                    if (inst.InstalacionId.HasValue)
                    {
                        var instalacion = await _instalacionRepo.GetByIdAsync(inst.InstalacionId.Value);
                        if (instalacion == null)
                            return ResultadoCrearCotizacion.Error($"Instalación con ID {inst.InstalacionId} no encontrada");

                        if (!instalacion.Activo)
                            return ResultadoCrearCotizacion.Error($"La instalación '{instalacion.Concepto}' no está activa");

                        cotizacion.AgregarInstalacionPredefinida(instalacion, inst.Cantidad);
                    }
                    else
                    {
                        cotizacion.AgregarInstalacion(
                            inst.Concepto,
                            inst.Descripcion ?? "",
                            inst.Cantidad,
                            inst.CostoUnitario
                        );
                    }
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    return ResultadoCrearCotizacion.Error(ex.Message);
                }
            }

            // ==========================================
            // PASO 8: VALIDAR REGLA DE DOMINIO - AUTORIZACIÓN
            // ==========================================
            if (cotizacion.RequiereAutorizacion)
            {
                // Aquí podrías:
                // - Enviar notificación al administrador
                // - Marcar la cotización como "Pendiente de autorización"
                // - Registrar en un log de auditoría
                Console.WriteLine($"⚠️ La cotización {numeroCotizacion} requiere autorización (Total: ${cotizacion.Total.Monto:N2} {cotizacion.Total.Moneda})");
            }

            // ==========================================
            // PASO 9: GUARDAR
            // ==========================================
            await _cotizacionRepo.AddAsync(cotizacion);

            return ResultadoCrearCotizacion.Exito(cotizacion);
        }
    }
}