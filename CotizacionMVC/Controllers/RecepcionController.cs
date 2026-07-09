using CotizacionMVC.Data;
using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Servicios.Aplicacion;
using CotizacionMVC.ViewModels.Recepcion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Controllers
{
    [Authorize(Roles = "Administrador,Recepcion")]
    public class RecepcionController : Controller
    {
        private readonly RecepcionServicio _recepcionServicio;
        private readonly IEmpresaRepository _empresaRepo;
        private readonly UserManager<Usuario> _userManager;
        private readonly IClienteRepository _clienteRepo;
        private readonly ApplicationDbContext _context;


        public RecepcionController(
    RecepcionServicio recepcionServicio,
    IEmpresaRepository empresaRepo,
    IClienteRepository clienteRepo,
    UserManager<Usuario> userManager,
    ApplicationDbContext context)
        {
            _recepcionServicio = recepcionServicio;
            _empresaRepo = empresaRepo;
            _userManager = userManager;
            _clienteRepo = clienteRepo;
            _context = context;
        }

        // GET: Recepcion/Registrar
        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            await CargarListasAsync();
            return View(new RegistrarClienteViewModel());
        }

        // POST: Recepcion/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarClienteViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                await CargarListasAsync();
                return View(modelo);
            }

            var usuarioActual = await _userManager.GetUserAsync(User);
            var esRecepcion = User.IsInRole("Recepcion") || User.IsInRole("Administrador");

            var resultado = await _recepcionServicio.RegistrarClienteAsync(
                modelo, usuarioActual!.Id, esRecepcion);

            if (!resultado.Exitoso)
            {
                if (resultado.EsDuplicado && resultado.ClienteExistente != null)
                {
                    TempData["ClienteExistenteNombre"] = resultado.ClienteExistente.Nombre;
                    TempData["ClienteExistenteFolio"] = resultado.ClienteExistente.Folio;
                    TempData["ClienteExistenteFecha"] = resultado.ClienteExistente.FechaRegistro.ToString("dd/MM/yyyy");
                }

                ModelState.AddModelError("", resultado.MensajeError!);
                await CargarListasAsync();
                return View(modelo);
            }

            TempData["MensajeExito"] = $"Cliente {resultado.Cliente!.Nombre} registrado exitosamente. Folio: {resultado.Cliente.Folio}";
            return RedirectToAction(nameof(Registrar));
        }

        // GET: Recepcion/BuscarCliente?telefono=xxx
        [HttpGet]
        public async Task<IActionResult> BuscarCliente(string telefono)
        {
            var clientes = await _recepcionServicio.BuscarPorTelefonoAsync(telefono);
            return Json(clientes.Select(c => new
            {
                c.Id,
                c.Nombre,
                Telefono = c.Contacto.Telefono,
                Correo = c.Contacto.Correo
            }));
        }

        // GET: Recepcion/Indice (Dashboard principal)
        // GET: Recepcion/Indice (Dashboard principal)
        [HttpGet]
        public async Task<IActionResult> Indice()
        {
            var clientes = await _clienteRepo.GetAllAsync();
            var ordenados = clientes.OrderByDescending(c => c.FechaRegistro).ToList();

            // Obtener Leads para saber el estado real de cada cliente
            var leads = await _context.Leads
                .Include(l => l.Cliente)
                .OrderByDescending(l => l.FechaCreacion)
                .ToListAsync();

            ViewBag.Leads = leads;

            return View(ordenados);
        }

        [HttpGet]
        public async Task<IActionResult> UltimosRegistros()
        {
            var clientes = await _clienteRepo.GetAllAsync();
            var ultimos = clientes
                .OrderByDescending(c => c.FechaRegistro)
                .Take(10)
                .Select(c => new
                {
                    c.Id,
                    c.Folio,
                    c.Nombre,
                    Telefono = c.Contacto.Telefono,
                    Origen = c.Origen.ToString(),
                    Fecha = c.FechaRegistro.ToString("dd/MM/yyyy HH:mm"),
                    Estado = _context.Leads
                        .Where(l => l.ClienteId == c.Id)
                        .OrderByDescending(l => l.FechaCreacion)
                        .Select(l => l.Estado.ToString())
                        .FirstOrDefault() ?? "SinAsignar",
                    VendedorAsignadoId = _context.Leads
                        .Where(l => l.ClienteId == c.Id)
                        .OrderByDescending(l => l.FechaCreacion)
                        .Select(l => l.VendedorAsignadoId)
                        .FirstOrDefault(),
                    observaciones = c.Observaciones
                });

            return Json(ultimos);
        }

        // GET: Recepcion/VerDetalles/5
        [HttpGet]
        public async Task<IActionResult> VerDetalles(Guid id)
        {
            var cliente = await _clienteRepo.GetByIdAsync(id);
            if (cliente == null)
                return NotFound("Cliente no encontrado");

            // Obtener nombre del vendedor si está asignado
            string? vendedorNombre = null;
            if (cliente.VendedorAsignadoId.HasValue)
            {
                var vendedor = await _userManager.FindByIdAsync(cliente.VendedorAsignadoId.Value.ToString());
                vendedorNombre = vendedor?.NombreCompleto ?? "No disponible";
            }

            // Extraer producto de las observaciones
            var producto = "No especificado";
            if (!string.IsNullOrWhiteSpace(cliente.Observaciones))
            {
                var partes = cliente.Observaciones.Split("Producto: ");
                if (partes.Length > 1)
                    producto = partes[1].Split(" |")[0];
            }

            var detalleHtml = $@"
                <div class='row'>
                    <div class='col-md-6'>
                        <h6 class='text-muted border-bottom pb-2'>Información del Cliente</h6>
                        <table class='table table-sm table-borderless'>
                            <tr><td class='fw-bold' style='width: 130px;'>Folio:</td><td><span class='badge bg-light text-dark border'>{cliente.Folio}</span></td></tr>
                            <tr><td class='fw-bold'>Nombre:</td><td><strong>{cliente.Nombre}</strong></td></tr>
                            <tr><td class='fw-bold'>Teléfono:</td><td><i class='fas fa-phone text-muted me-1'></i>{cliente.Contacto.Telefono}</td></tr>
                            <tr><td class='fw-bold'>Tel. Móvil:</td><td>{(string.IsNullOrWhiteSpace(cliente.Contacto.TelefonoMovil) ? "<span class='text-muted'>N/A</span>" : cliente.Contacto.TelefonoMovil)}</td></tr>
                            <tr><td class='fw-bold'>Correo:</td><td>{(string.IsNullOrWhiteSpace(cliente.Contacto.Correo) ? "<span class='text-muted'>N/A</span>" : cliente.Contacto.Correo)}</td></tr>
                        </table>
                    </div>
                    <div class='col-md-6'>
                        <h6 class='text-muted border-bottom pb-2'>Datos de Registro</h6>
                        <table class='table table-sm table-borderless'>
                            <tr><td class='fw-bold' style='width: 130px;'>Origen:</td><td><span class='badge bg-light text-dark'>{cliente.Origen}</span></td></tr>
                            <tr><td class='fw-bold'>Estado:</td><td><span class='badge bg-{GetEstadoColor(cliente.Estado)}'>{cliente.Estado}</span></td></tr>
                            <tr><td class='fw-bold'>Producto:</td><td>{producto}</td></tr>
                            <tr><td class='fw-bold'>Vendedor:</td><td>{(vendedorNombre != null ? $"<i class='fas fa-user-check text-success me-1'></i>{vendedorNombre}" : "<span class='text-warning'><i class='fas fa-clock me-1'></i>Sin asignar</span>")}</td></tr>
                            <tr><td class='fw-bold'>Fecha Registro:</td><td>{cliente.FechaRegistro:dd/MM/yyyy HH:mm}</td></tr>
                        </table>
                    </div>
                </div>
                <div class='row mt-3'>
                    <div class='col-12'>
                        <h6 class='text-muted border-bottom pb-2'>Observaciones</h6>
                        <div class='p-3 bg-light rounded' style='min-height: 60px;'>
                            {(string.IsNullOrWhiteSpace(cliente.Observaciones) ? "<span class='text-muted'>Sin observaciones</span>" : cliente.Observaciones)}
                        </div>
                    </div>
                </div>";

            return Content(detalleHtml, "text/html");
        }

        // GET: Recepcion/ObtenerVendedores
        [HttpGet]
        public async Task<IActionResult> ObtenerVendedores()
        {
            var vendedores = await _userManager.GetUsersInRoleAsync("Vendedor");
            var activos = vendedores
                .Where(v => v.Activo)
                .Select(v => new
                {
                    v.Id,
                    v.NombreCompleto,
                    v.Email
                })
                .OrderBy(v => v.NombreCompleto);

            return Json(activos);
        }

        // POST: Recepcion/AsignarVendedor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarVendedor(Guid clienteId, Guid vendedorId)
        {
            try
            {
                var resultado = await _recepcionServicio.AsignarVendedorAsync(clienteId, vendedorId);

                if (resultado.Exitoso)
                    return Json(new { success = true, message = "Vendedor asignado exitosamente" });
                else
                    return Json(new { success = false, message = resultado.MensajeError });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // POST: Recepcion/MarcarNoCotizable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarNoCotizable(Guid clienteId, string motivo, string? comentario)
        {
            try
            {
                if (!Enum.TryParse<MotivoNoCotizable>(motivo, out var motivoEnum))
                    return Json(new { success = false, message = "Motivo no válido" });

                var resultado = await _recepcionServicio.MarcarNoCotizableAsync(clienteId, motivoEnum, comentario);

                if (resultado.Exitoso)
                    return Json(new { success = true, message = "Cliente marcado como no cotizable" });
                else
                    return Json(new { success = false, message = resultado.MensajeError });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }

        // ==================== MÉTODOS AUXILIARES ====================

        private async Task CargarListasAsync()
        {
            ViewBag.Empresas = await _empresaRepo.GetAllAsync();
            var vendedores = await _userManager.GetUsersInRoleAsync("Vendedor");
            ViewBag.Vendedores = vendedores.Where(v => v.Activo).ToList();
        }

        private string GetEstadoColor(EstadoCliente estado)
        {
            return estado switch
            {
                EstadoCliente.SinAsignar => "warning",
                EstadoCliente.Asignado => "info",
                EstadoCliente.Contactado => "primary",
                EstadoCliente.Cotizado => "success",
                EstadoCliente.NoCotizable => "danger",
                EstadoCliente.Cerrado => "dark",
                _ => "secondary"
            };
        }
    }
}