using Microsoft.AspNetCore.Mvc;
using AppsTigreUruguay.Areas.TrackingComercial.Models;
using System.Text.Json;

namespace AppsTigreUruguay.Areas.TrackingComercial.Controllers
{
    [Area("TrackingComercial")]
    public class FacturaController : Controller
    {
        private readonly IWebHostEnvironment _env;

        public FacturaController(IWebHostEnvironment env)
        {
            _env = env;
        }

        // GET: /TrackingComercial/Factura/UploadExcel
        [HttpGet]
        public IActionResult UploadExcel()
        {
            // Verifica que el usuario esté logueado
            var usuario = HttpContext.Session.GetString("UsuarioTrackingComercial");
            if (string.IsNullOrEmpty(usuario))
            {
                return RedirectToAction("Login", "Home");
            }

            return View();
        }

        // POST: /TrackingComercial/Factura/GuardarFacturas
        [HttpPost]
        public IActionResult GuardarFacturas([FromBody] List<FacturaDTO> facturas)
        {
            // Carpeta segura para guardar JSON (fuera de wwwroot)
            var dataFolder = Path.Combine(_env.ContentRootPath, "Data", "TrackingComercial");

            // Nombre del archivo
            var filePath = Path.Combine(dataFolder, "facturas.json");

            try
            {
                if (facturas == null || facturas.Count == 0)
                    return BadRequest("No se recibieron datos o el JSON está mal formado.");

                // Crear carpeta si no existe
                if (!Directory.Exists(dataFolder))
                {
                    Directory.CreateDirectory(dataFolder);
                }

                // Serializar a JSON
                var json = JsonSerializer.Serialize(facturas, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                // Guardar el archivo
                System.IO.File.WriteAllText(filePath, json);

                return Ok(new
                {
                    mensaje = "Archivo JSON guardado correctamente",
                    ruta = filePath,
                    totalRegistros = facturas.Count
                });
            }
            catch (Exception ex)
            {
                // Devuelve la ruta y el error para debugging
                return StatusCode(500, new
                {
                    mensaje = "Error al guardar el JSON",
                    ruta = filePath,
                    error = ex.Message
                });
            }
        }
    }
}
