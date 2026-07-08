using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion;
using CotizacionMVC.ViewModels.Recepcion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers
{
    [Authorize(Roles = "Administrador,Recepcion")]
    public class RecepcionController : Controller
    {
        private readonly RecepcionServicio _recepcionServicio;
        private readonly IEmpresaRepository _empresaRepo;
        private readonly UserManager<Usuario> _userManager;
        private readonly IClienteRepository _clienteRepo;  


        public RecepcionController(
            RecepcionServicio recepcionServicio,
            IEmpresaRepository empresaRepo, IClienteRepository clienteRepo,
            UserManager<Usuario> userManager )
        {
            _recepcionServicio = recepcionServicio;
            _empresaRepo = empresaRepo;
            _userManager = userManager;
            _clienteRepo = clienteRepo; 

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
                // ✅ Si es duplicado, mostrar datos del cliente existente
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

        // Método auxiliar para cargar listas
        private async Task CargarListasAsync()
        {
            ViewBag.Empresas = await _empresaRepo.GetAllAsync();
            var vendedores = await _userManager.GetUsersInRoleAsync("Vendedor");
            ViewBag.Vendedores = vendedores.Where(v => v.Activo).ToList();
        }
        // GET: Recepcion/Indice (Dashboard principal)
        [HttpGet]
        public async Task<IActionResult> Indice()
        {
            // Obtener todos los clientes ordenados por fecha
            var clientes = await _clienteRepo.GetAllAsync();
            var ordenados = clientes.OrderByDescending(c => c.FechaRegistro).ToList();

            return View(ordenados);
        }

        // GET: Recepcion/UltimosRegistros (para AJAX)
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
                    Estado = c.Estado.ToString(),
                    Origen = c.Origen.ToString(),
                    Fecha = c.FechaRegistro.ToString("dd/MM/yyyy HH:mm"),
                    VendedorAsignadoId = c.VendedorAsignadoId
                });

            return Json(ultimos);
        }

    }
}