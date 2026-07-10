using CotizacionMVC.Models.Entidades;

using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Recepcion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace CotizacionMVC.Controllers
{
    [Authorize]
    public class RecepcionController : Controller
    {
        private readonly IRecepcionServicio _recepcionServicio;
        private readonly UserManager<Usuario> _userManager;

        public RecepcionController(
            IRecepcionServicio recepcionServicio,
            UserManager<Usuario> userManager)
        {
            _recepcionServicio = recepcionServicio;
            _userManager = userManager;
        }

        // GET: Recepcion/Registrar
        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            ViewBag.Empresas = await _recepcionServicio.ObtenerEmpresasAsync();
            ViewBag.Vendedores = await _recepcionServicio.ObtenerVendedoresActivosAsync();
            ViewBag.EsRecepcion = User.IsInRole("Recepcion") || User.IsInRole("Administrador");
            return View(new RegistrarClienteViewModel());
        }

        // POST: Recepcion/Registrar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarClienteViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Empresas = await _recepcionServicio.ObtenerEmpresasAsync();
                ViewBag.Vendedores = await _recepcionServicio.ObtenerVendedoresActivosAsync();
                ViewBag.EsRecepcion = User.IsInRole("Recepcion") || User.IsInRole("Administrador");
                return View(modelo);
            }

            var usuarioActual = await _userManager.GetUserAsync(User);
            var esRecepcion = User.IsInRole("Recepcion") || User.IsInRole("Administrador");

            var resultado = await _recepcionServicio.RegistrarClienteAsync(
                modelo, usuarioActual!.Id, esRecepcion);

            if (!resultado.Exitoso)
            {
                ModelState.AddModelError("", resultado.MensajeError!);
                ViewBag.Empresas = await _recepcionServicio.ObtenerEmpresasAsync();
                ViewBag.Vendedores = await _recepcionServicio.ObtenerVendedoresActivosAsync();
                ViewBag.EsRecepcion = User.IsInRole("Recepcion") || User.IsInRole("Administrador");
                return View(modelo);
            }

            TempData["MensajeExito"] = $"Cliente {resultado.Cliente!.Nombre} registrado exitosamente. Folio: {resultado.Cliente.Folio}";

            if (User.IsInRole("Vendedor"))
                return RedirectToAction("Indice", "Cotizacion");

            return RedirectToAction(nameof(Registrar));
        }

        // GET: Recepcion/BuscarCliente
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

        // GET: Recepcion/Indice
        [HttpGet]
        public async Task<IActionResult> Indice()
        {
            var clientes = await _recepcionServicio.ObtenerDashboardAsync();
            return View(clientes);
        }

        // GET: Recepcion/UltimosRegistros
        [HttpGet]
        public async Task<IActionResult> UltimosRegistros()
        {
            var ultimos = await _recepcionServicio.ObtenerUltimosRegistrosAsync();
            return Json(ultimos);
        }

        // GET: Recepcion/VerDetalles
        [HttpGet]
        public async Task<IActionResult> VerDetalles(Guid id)
        {
            var detalle = await _recepcionServicio.ObtenerDetalleClienteAsync(id);
            if (detalle == null)
                return NotFound("Cliente no encontrado");

            var detalleHtml = $@"
                <div class='row'>
                    <div class='col-md-6'>
                        <h6 class='text-muted border-bottom pb-2'>Información del Cliente</h6>
                        <table class='table table-sm table-borderless'>
                            <tr><td class='fw-bold' style='width: 130px;'>Folio:</td><td><span class='badge bg-light text-dark border'>{detalle.Folio}</span></td></tr>
                            <tr><td class='fw-bold'>Nombre:</td><td><strong>{detalle.Nombre}</strong></td></tr>
                            <tr><td class='fw-bold'>Teléfono:</td><td><i class='fas fa-phone text-muted me-1'></i>{detalle.Telefono}</td></tr>
                            <tr><td class='fw-bold'>Tel. Móvil:</td><td>{(string.IsNullOrWhiteSpace(detalle.TelefonoMovil) ? "<span class='text-muted'>N/A</span>" : detalle.TelefonoMovil)}</td></tr>
                            <tr><td class='fw-bold'>Correo:</td><td>{(string.IsNullOrWhiteSpace(detalle.Correo) ? "<span class='text-muted'>N/A</span>" : detalle.Correo)}</td></tr>
                        </table>
                    </div>
                    <div class='col-md-6'>
                        <h6 class='text-muted border-bottom pb-2'>Datos de Registro</h6>
                        <table class='table table-sm table-borderless'>
                            <tr><td class='fw-bold' style='width: 130px;'>Origen:</td><td><span class='badge bg-light text-dark'>{detalle.Origen}</span></td></tr>
                            <tr><td class='fw-bold'>Estado:</td><td><span class='badge bg-info'>{detalle.Estado}</span></td></tr>
                            <tr><td class='fw-bold'>Producto:</td><td>{detalle.Producto}</td></tr>
                            <tr><td class='fw-bold'>Vendedor:</td><td>{(detalle.VendedorNombre != null ? $"<i class='fas fa-user-check text-success me-1'></i>{detalle.VendedorNombre}" : "<span class='text-warning'><i class='fas fa-clock me-1'></i>Sin asignar</span>")}</td></tr>
                            <tr><td class='fw-bold'>Fecha Registro:</td><td>{detalle.FechaRegistro:dd/MM/yyyy HH:mm}</td></tr>
                        </table>
                    </div>
                </div>
                <div class='row mt-3'>
                    <div class='col-12'>
                        <h6 class='text-muted border-bottom pb-2'>Observaciones</h6>
                        <div class='p-3 bg-light rounded' style='min-height: 60px;'>
                            {(string.IsNullOrWhiteSpace(detalle.Observaciones) ? "<span class='text-muted'>Sin observaciones</span>" : detalle.Observaciones)}
                        </div>
                    </div>
                </div>";

            return Content(detalleHtml, "text/html");
        }

        // GET: Recepcion/ObtenerVendedores
        [HttpGet]
        public async Task<IActionResult> ObtenerVendedores()
        {
            var vendedores = await _recepcionServicio.ObtenerVendedoresActivosAsync();
            return Json(vendedores);
        }

        // POST: Recepcion/AsignarVendedor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarVendedor(Guid clienteId, Guid vendedorId)
        {
            var resultado = await _recepcionServicio.AsignarVendedorAsync(clienteId, vendedorId);

            if (resultado.Exitoso)
                return Json(new { success = true, message = "Vendedor asignado exitosamente" });

            return Json(new { success = false, message = resultado.MensajeError });
        }

        // POST: Recepcion/MarcarNoCotizable
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarNoCotizable(Guid clienteId, string motivo, string? comentario)
        {
            var resultado = await _recepcionServicio.MarcarNoCotizableAsync(clienteId, motivo, comentario);

            if (resultado.Exitoso)
                return Json(new { success = true, message = "Cliente marcado como no cotizable" });

            return Json(new { success = false, message = resultado.MensajeError });
        }
    }
}