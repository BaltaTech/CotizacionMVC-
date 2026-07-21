using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Recepcion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers
{
    [Authorize(Roles = "Administrador,Recepcion,Vendedor")]
    public class RecepcionController : Controller
    {
        private readonly IRecepcionServicio _recepcionServicio;
        private readonly UserManager<Usuario> _userManager;
        private readonly IAutorizacionServicio _autorizacionServicio;

        public RecepcionController(
            IRecepcionServicio recepcionServicio,
            UserManager<Usuario> userManager,
            IAutorizacionServicio autorizacionServicio)
        {
            _recepcionServicio = recepcionServicio;
            _userManager = userManager;
            _autorizacionServicio = autorizacionServicio;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Recepcion,Vendedor")]
        public async Task<IActionResult> Registrar()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            var esRecepcion = usuarioActual != null &&
                (await _autorizacionServicio.EsRecepcionAsync(usuarioActual.Id) ||
                 await _autorizacionServicio.EsAdminAsync(usuarioActual.Id));

            ViewBag.Empresas = await _recepcionServicio.ObtenerEmpresasAsync();
            ViewBag.Vendedores = await _recepcionServicio.ObtenerVendedoresActivosAsync();
            ViewBag.EsRecepcion = esRecepcion;
            return View(new RegistrarClienteViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador,Recepcion,Vendedor")]
        public async Task<IActionResult> Registrar(RegistrarClienteViewModel modelo)
        {
            var usuarioActual = await _userManager.GetUserAsync(User);

            if (!ModelState.IsValid)
            {
                var esRecepcion = usuarioActual != null &&
                    (await _autorizacionServicio.EsRecepcionAsync(usuarioActual.Id) ||
                     await _autorizacionServicio.EsAdminAsync(usuarioActual.Id));

                ViewBag.Empresas = await _recepcionServicio.ObtenerEmpresasAsync();
                ViewBag.Vendedores = await _recepcionServicio.ObtenerVendedoresActivosAsync();
                ViewBag.EsRecepcion = esRecepcion;
                return View(modelo);
            }

            var resultado = await _recepcionServicio.RegistrarClienteAsync(
                modelo, usuarioActual!.Id);

            if (!resultado.Exitoso)
            {
                var esRecepcion = await _autorizacionServicio.EsRecepcionAsync(usuarioActual.Id) ||
                                  await _autorizacionServicio.EsAdminAsync(usuarioActual.Id);

                ModelState.AddModelError("", resultado.MensajeError!);
                ViewBag.Empresas = await _recepcionServicio.ObtenerEmpresasAsync();
                ViewBag.Vendedores = await _recepcionServicio.ObtenerVendedoresActivosAsync();
                ViewBag.EsRecepcion = esRecepcion;
                return View(modelo);
            }

            TempData["MensajeExito"] = $"Cliente {resultado.Cliente!.Nombre} registrado exitosamente. Folio: {resultado.Cliente.Folio}";

            if (await _autorizacionServicio.EsVendedorAsync(usuarioActual.Id))
                return RedirectToAction("Indice", "Cotizacion");

            return RedirectToAction(nameof(Registrar));
        }

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

        [HttpGet]
        public async Task<IActionResult> Indice()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
                return RedirectToAction("Login", "Autenticacion");

            var clientes = await _recepcionServicio.ObtenerDashboardAsync(usuarioActual.Id);
            return View(clientes);
        }

        [HttpGet]
        public async Task<IActionResult> UltimosRegistros()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
                return Json(new List<object>());

            var ultimos = await _recepcionServicio.ObtenerUltimosRegistrosAsync(usuarioActual.Id);
            return Json(ultimos);
        }

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

        [HttpGet]
        public async Task<IActionResult> ObtenerVendedores()
        {
            var vendedores = await _recepcionServicio.ObtenerVendedoresActivosAsync();
            return Json(vendedores);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AsignarVendedor(Guid clienteId, Guid vendedorId)
        {
            var resultado = await _recepcionServicio.AsignarVendedorAsync(clienteId, vendedorId);

            if (resultado.Exitoso)
                return Json(new { success = true, message = "Vendedor asignado exitosamente" });

            return Json(new { success = false, message = resultado.MensajeError });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarNoCotizable(Guid clienteId, string motivo, string? comentario)
        {
            var resultado = await _recepcionServicio.MarcarNoCotizableAsync(clienteId, motivo, comentario);

            if (resultado.Exitoso)
                return Json(new { success = true, message = "Cliente marcado como no cotizable" });

            return Json(new { success = false, message = resultado.MensajeError });
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerContadoresEstado()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
                return Json(new { total = 0, sinAsignar = 0, pendientesCotizar = 0, cotizados = 0, noCotizables = 0 });

            var clientes = await _recepcionServicio.ObtenerDashboardAsync(usuarioActual.Id);

            var contadores = new
            {
                total = clientes.Count,
                sinAsignar = clientes.Count(c => c.Estado == "SinAsignar"),
                pendientesCotizar = clientes.Count(c => c.Estado == "Asignado" || c.Estado == "Contactado"),
                cotizados = clientes.Count(c => c.Estado == "Cotizado"),
                noCotizables = clientes.Count(c => c.Estado == "NoCotizable")
            };

            return Json(contadores);
        }
    }
}