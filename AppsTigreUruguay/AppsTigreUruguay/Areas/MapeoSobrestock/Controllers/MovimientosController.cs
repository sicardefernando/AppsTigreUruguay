using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using AppsTigreUruguay.Areas.MapeoSobrestock.Models;

namespace AppsTigreUruguay.Areas.MapeoSobrestock.Controllers
{
    [Area("MapeoSobrestock")]
    public class MovimientosController : Controller
    {
        private readonly string dataFolder;

        public MovimientosController()
        {
            dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data", "MapeoSobrestock");
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);
        }

        public IActionResult Index()
        {
            // Validar sesión
            var usuario = HttpContext.Session.GetString("UsuarioMapeoSobrestock");
            if (string.IsNullOrEmpty(usuario))
                return RedirectToAction("Login", "Account");

            var rutaLog = Path.Combine(dataFolder, "movimientos.json");
            List<MovimientoUsuario> movimientos = new List<MovimientoUsuario>();

            if (System.IO.File.Exists(rutaLog))
            {
                var contenido = System.IO.File.ReadAllText(rutaLog);
                movimientos = JsonSerializer.Deserialize<List<MovimientoUsuario>>(contenido) ?? new List<MovimientoUsuario>();
            }

            // Ordenar por fecha descendente
            movimientos.Sort((a, b) => b.FechaHora.CompareTo(a.FechaHora));

            return View(movimientos);
        }
    }
}
