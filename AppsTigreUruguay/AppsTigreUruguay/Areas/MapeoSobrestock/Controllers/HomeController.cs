using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClosedXML.Excel;
using AppsTigreUruguay.Areas.MapeoSobrestock.Models;
using Microsoft.Extensions.Configuration;

namespace AppsTigreUruguay.Areas.MapeoSobrestock.Controllers
{
    [Area("MapeoSobrestock")]
    public class HomeController : Controller
    {
        private readonly string dataFolder;
        private readonly string connectionString;

        public HomeController(IConfiguration configuration)
        {
            dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data", "MapeoSobrestock");
            if (!Directory.Exists(dataFolder))
                Directory.CreateDirectory(dataFolder);

            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IActionResult Index()
        {
            var usuario = HttpContext.Session.GetString("UsuarioMapeoSobrestock");
            if (string.IsNullOrEmpty(usuario))
                return RedirectToAction("Login", "Account");

            var pasillos = LeerPasillos();

            var ubicaciones = LeerUbicaciones();

            var viewModel = new VistaPrincipalViewModel
            {
                Pasillos = pasillos,
                UbicacionesProductos = ubicaciones
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult AgregarCodigo([FromBody] UbicacionProducto model)
        {
            if (model == null || string.IsNullOrEmpty(model.Ubicacion) || string.IsNullOrEmpty(model.CodigoProducto))
                return BadRequest(new { success = false, message = "Datos inválidos" });

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var query = @"
                    IF NOT EXISTS (SELECT 1 FROM UbicacionesProductos WHERE Ubicacion = @Ubicacion AND CodigoProducto = @CodigoProducto)
                        INSERT INTO UbicacionesProductos (Ubicacion, CodigoProducto) VALUES (@Ubicacion, @CodigoProducto)";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Ubicacion", model.Ubicacion);
                    cmd.Parameters.AddWithValue("@CodigoProducto", model.CodigoProducto);
                    cmd.ExecuteNonQuery();
                }
            }

            var usuario = HttpContext.Session.GetString("UsuarioMapeoSobrestock") ?? "Invitado";
            LoggerMovimientos.RegistrarMovimiento(usuario, "Agregar Código", $"Código: {model.CodigoProducto}, Ubicación: {model.Ubicacion}");

            return Json(new { success = true });
        }

        [HttpPost]
        public IActionResult EliminarCodigo([FromBody] UbicacionProducto model)
        {
            if (model == null || string.IsNullOrEmpty(model.Ubicacion) || string.IsNullOrEmpty(model.CodigoProducto))
                return BadRequest(new { success = false, message = "Datos inválidos" });

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var query = "DELETE FROM UbicacionesProductos WHERE Ubicacion = @Ubicacion AND CodigoProducto = @CodigoProducto";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Ubicacion", model.Ubicacion);
                    cmd.Parameters.AddWithValue("@CodigoProducto", model.CodigoProducto);
                    cmd.ExecuteNonQuery();
                }
            }

            var usuario = HttpContext.Session.GetString("UsuarioMapeoSobrestock") ?? "Invitado";
            LoggerMovimientos.RegistrarMovimiento(usuario, "Eliminar Código", $"Código: {model.CodigoProducto}, Ubicación: {model.Ubicacion}");

            return Json(new { success = true });
        }

        public IActionResult DescargarExcel()
        {
            try
            {
                var pasillos = LeerPasillos();
                var ubicaciones = LeerUbicaciones();

                DataTable dt = new DataTable();
                dt.Columns.Add("Pasillo", typeof(string));
                dt.Columns.Add("Posición", typeof(string));
                dt.Columns.Add("CódigoProducto", typeof(string));

                foreach (var ubicacion in ubicaciones)
                {
                    var partes = ubicacion.Ubicacion.Split('-');
                    if (partes.Length != 3) continue;

                    string pasillo = partes[0];
                    if (!int.TryParse(partes[1], out int nivelBase)) continue;
                    if (!int.TryParse(partes[2], out int columnaIndex)) continue;

                    var pasilloData = pasillos.FirstOrDefault(p => p.nombrePasillo == pasillo);
                    if (pasilloData == null || columnaIndex >= pasilloData.nomColumnas.Count) continue;

                    int totalNiveles = pasilloData.cantNiveles;
                    int nivelVisual = totalNiveles - nivelBase;
                    string nombreColumna = pasilloData.nomColumnas[columnaIndex]?.ToString() ?? "";
                    string posicion = $"Nivel {nivelVisual} - Col {nombreColumna}";

                    dt.Rows.Add(pasillo, posicion, ubicacion.CodigoProducto);
                }

                var ms = new MemoryStream();
                using (var wb = new XLWorkbook())
                {
                    wb.Worksheets.Add(dt, "Ubicaciones");
                    wb.SaveAs(ms, false);
                }
                ms.Position = 0;

                return File(ms, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Ubicaciones.xlsx");
            }
            catch (Exception ex)
            {
                var logPath = Path.Combine(dataFolder, "ErrorDescargaExcel.log");
                System.IO.File.AppendAllText(logPath, $"{System.DateTime.Now:s} - {ex}{System.Environment.NewLine}");
                return Content($"<h2>Error al generar Excel</h2><pre>{ex}</pre>", "text/html");
            }
        }

        // Método helper para leer pasillos JSON
        private List<Pasillo> LeerPasillos()
        {
            var rutaPasillos = Path.Combine(dataFolder, "layoutPasillos.json");
            var pasillos = new List<Pasillo>();
            if (System.IO.File.Exists(rutaPasillos))
            {
                var contenido = System.IO.File.ReadAllText(rutaPasillos);
                var opciones = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
                };
                pasillos = JsonSerializer.Deserialize<List<Pasillo>>(contenido, opciones) ?? new List<Pasillo>();
            }
            return pasillos;
        }

        // Método helper para leer ubicaciones desde SQL
        private List<UbicacionProducto> LeerUbicaciones()
        {
            var ubicaciones = new List<UbicacionProducto>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var query = "SELECT Ubicacion, CodigoProducto FROM UbicacionesProductos";
                using (var cmd = new SqlCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ubicaciones.Add(new UbicacionProducto
                        {
                            Ubicacion = reader.GetString(0),
                            CodigoProducto = reader.GetString(1)
                        });
                    }
                }
            }
            return ubicaciones;
        }

        [HttpGet]
        public IActionResult ObtenerUbicaciones()
        {
            try
            {
                var ubicaciones = LeerUbicaciones();
                return Json(ubicaciones);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
    }
}
