using CotizacionMVC.Data;
using CotizacionMVC.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Controllers
{
    public class EmpresaController : Controller
    {
        private readonly ApplicationDbContext _contextoBaseDatos;

        public EmpresaController(ApplicationDbContext contextoBaseDatos)
        {
            _contextoBaseDatos = contextoBaseDatos;
        }

        // GET: Empresa/Indice
        public async Task<IActionResult> Indice()
        {
            var empresas = await _contextoBaseDatos.Empresas
                .OrderBy(e => e.NombreComercial)
                .ToListAsync();

            return View(empresas);
        }

        // GET: Empresa/Editar/5
        // Solo administrador - Formulario para editar empresa
        public async Task<IActionResult> Editar(Guid? id)
        {
            if (id == null)
            {
                return NotFound("No se proporcionó un identificador de empresa");
            }

            var empresa = await _contextoBaseDatos.Empresas.FindAsync(id);

            if (empresa == null)
            {
                return NotFound($"No se encontró la empresa con ID {id}");
            }

            return View(empresa);
        }

        // POST: Empresa/Editar/5
        // Solo administrador - Guardar cambios de la empresa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Guid id, Empresa empresa)
        {
            if (id != empresa.Id)
            {
                return NotFound("El identificador de la empresa no coincide");
            }

            if (!ModelState.IsValid)
            {
                return View(empresa);
            }

            try
            {
                var empresaExistente = await _contextoBaseDatos.Empresas.FindAsync(id);

                if (empresaExistente == null)
                {
                    return NotFound($"No se encontró la empresa con ID {id}");
                }

                // Actualizar solo los campos permitidos
                empresaExistente.ActualizarUtilidades(
                    empresa.UtilidadEmpresaPorcentaje,
                    empresa.UtilidadVendedorPorcentaje
                );

                // Actualizar datos de contacto
                empresaExistente.ActualizarContacto(empresa.TelefonoContacto, empresa.CorreoContacto);

                // Actualizar identidad visual
                empresaExistente.ConfigurarIdentidadVisual(
                    empresa.LogoUrl ?? "",
                    empresa.ColorPrimario ?? "",
                    empresa.ColorSecundario ?? "",
                    empresa.PlantillaPdfNombre ?? "",
                    empresa.Eslogan ?? ""
                );

                await _contextoBaseDatos.SaveChangesAsync();

                TempData["MensajeExito"] = $"Empresa {empresaExistente.NombreComercial} actualizada exitosamente";
                return RedirectToAction(nameof(Indice));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmpresaExiste(id))
                {
                    return NotFound($"La empresa con ID {id} ya no existe");
                }
                throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                return View(empresa);
            }
        }

        // POST: Empresa/CambiarEmpresaActiva
        // Cualquier usuario autenticado - Cambia la empresa activa en sesión
        [HttpPost]
       // [Authorize]
        public async Task<IActionResult> CambiarEmpresaActiva(Guid empresaId, string? returnUrl = null)
        {
            var empresa = await _contextoBaseDatos.Empresas.FindAsync(empresaId);

            if (empresa == null)
            {
                return NotFound("No se encontró la empresa seleccionada");
            }

            if (!empresa.Activa)
            {
                TempData["MensajeError"] = "La empresa seleccionada no está activa";
                return RedirectToLocal(returnUrl);
            }

            // Guardar empresa activa en sesión
            HttpContext.Session.SetString("EmpresaActivaId", empresa.Id.ToString());
            HttpContext.Session.SetString("EmpresaActivaNombre", empresa.NombreComercial);
            HttpContext.Session.SetString("EmpresaActivaSlug", empresa.Slug);
            HttpContext.Session.SetString("EmpresaEsExclusivaTrane", empresa.EsExclusivaTrane.ToString());

            TempData["MensajeExito"] = $"Ahora estás trabajando en: {empresa.NombreComercial}";
            return RedirectToLocal(returnUrl);
        }

        public async Task<Empresa?> ObtenerEmpresaActual()
        {
            var empresaIdString = HttpContext.Session.GetString("EmpresaActivaId");

            if (string.IsNullOrEmpty(empresaIdString))
            {
                // Si no hay empresa en sesión, obtener la primera activa
                var primeraEmpresa = await _contextoBaseDatos.Empresas
                    .FirstOrDefaultAsync(e => e.Activa);

                if (primeraEmpresa != null)
                {
                    HttpContext.Session.SetString("EmpresaActivaId", primeraEmpresa.Id.ToString());
                    HttpContext.Session.SetString("EmpresaActivaNombre", primeraEmpresa.NombreComercial);
                    HttpContext.Session.SetString("EmpresaActivaSlug", primeraEmpresa.Slug);
                    HttpContext.Session.SetString("EmpresaEsExclusivaTrane", primeraEmpresa.EsExclusivaTrane.ToString());
                    return primeraEmpresa;
                }

                return null;
            }

            var empresaId = Guid.Parse(empresaIdString);
            var empresa = await _contextoBaseDatos.Empresas.FindAsync(empresaId);

            if (empresa == null || !empresa.Activa)
            {
                // La empresa guardada en sesión ya no es válida
                HttpContext.Session.Remove("EmpresaActivaId");
                return await ObtenerEmpresaActual();
            }

            return empresa;
        }

        private bool EmpresaExiste(Guid id)
        {
            return _contextoBaseDatos.Empresas.Any(e => e.Id == id);
        }

        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Indice", "Equipo");
        }
    }
}