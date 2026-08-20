using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Recepcion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers.MVC
{
    [Authorize(Roles = "Administrador,Recepcion,Vendedor")]
    public class RecepcionController : Controller
    {
        private readonly IRecepcionServicio _recepcionServicio;
        private readonly IUserContextService _userContextService;

        public RecepcionController(
            IRecepcionServicio recepcionServicio,
            IUserContextService userContextService)
        {
            _recepcionServicio = recepcionServicio;
            _userContextService = userContextService;
        }

        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            var esRecepcion = await _userContextService.IsUserInRoleAsync("Recepcion") ||
                              await _userContextService.IsUserInRoleAsync("Administrador");

            ViewBag.Empresas = await _recepcionServicio.ObtenerEmpresasAsync();
            ViewBag.Vendedores = await _recepcionServicio.ObtenerVendedoresActivosAsync();
            ViewBag.EsRecepcion = esRecepcion;

            return View(new RegistrarClienteViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(RegistrarClienteViewModel modelo)
        {
            var usuarioActual = await _userContextService.GetCurrentUserAsync();
            var esRecepcion = await _userContextService.IsUserInRoleAsync("Recepcion") ||
                              await _userContextService.IsUserInRoleAsync("Administrador");

            if (!ModelState.IsValid)
            {
                ViewBag.Empresas = await _recepcionServicio.ObtenerEmpresasAsync();
                ViewBag.Vendedores = await _recepcionServicio.ObtenerVendedoresActivosAsync();
                ViewBag.EsRecepcion = esRecepcion;
                return View(modelo);
            }

            var resultado = await _recepcionServicio.RegistrarClienteAsync(modelo, usuarioActual.Id);

            if (!resultado.Exitoso)
            {
                ModelState.AddModelError("", resultado.MensajeError ?? "Error al registrar cliente");
                ViewBag.Empresas = await _recepcionServicio.ObtenerEmpresasAsync();
                ViewBag.Vendedores = await _recepcionServicio.ObtenerVendedoresActivosAsync();
                ViewBag.EsRecepcion = esRecepcion;
                return View(modelo);
            }

            TempData["MensajeExito"] = $"Cliente {resultado.Cliente!.Nombre} registrado exitosamente. Folio: {resultado.Cliente.Folio}";

            if (await _userContextService.IsUserInRoleAsync("Vendedor"))
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
            var usuarioActual = await _userContextService.GetCurrentUserAsync();
            var clientes = await _recepcionServicio.ObtenerDashboardAsync(usuarioActual.Id);
            return View(clientes);
        }

        [HttpGet]
        public async Task<IActionResult> UltimosRegistros()
        {
            var usuarioActual = await _userContextService.GetCurrentUserAsync();
            var ultimos = await _recepcionServicio.ObtenerUltimosRegistrosAsync(usuarioActual.Id);
            return Json(ultimos);
        }

        [HttpGet]
        public async Task<IActionResult> VerDetalles(Guid id)
        {
            var detalle = await _recepcionServicio.ObtenerDetalleClienteAsync(id);
            if (detalle == null)
                return NotFound("Cliente no encontrado");

            return PartialView("_DetalleCliente", detalle);
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
            var usuarioActual = await _userContextService.GetCurrentUserAsync();
            var clientes = await _recepcionServicio.ObtenerDashboardAsync(usuarioActual.Id);

            return Json(new
            {
                total = clientes.Count,
                sinAsignar = clientes.Count(c => c.Estado == "SinAsignar"),
                pendientesCotizar = clientes.Count(c => c.Estado == "Asignado" || c.Estado == "Contactado"),
                cotizados = clientes.Count(c => c.Estado == "Cotizado"),
                noCotizables = clientes.Count(c => c.Estado == "NoCotizable")
            });
        }
    }
}