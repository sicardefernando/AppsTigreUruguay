using Microsoft.AspNetCore.Mvc;
using AppsTigreUruguay.Areas.TrackingComercial.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;


namespace AppsTigreUruguay.Areas.TrackingComercial.Controllers
{
    [Area("TrackingComercial")]
    public class ReporteController : Controller
    {
        private static List<FacturaDTO> ventas = new List<FacturaDTO>();

        private static readonly List<string> productos = new List<string>
        {
            "Acc. Sanitarios", "Adhesivo", "Canaletas Piso", "Canaletas Techo", "Drenaje",
            "Junta Elastica", "Ramat", "Tubos de PVC", "Ralo", "Desague EG", "Fusion",
            "Fusion KL", "Tanques", "Soldable", "Valvulas", "TigreFire", "Goteo", "Irriga",
            "ADS", "PBA", "PEAD", "VT", "Pocero"
        };

        private static List<string> clientes = new List<string>();

        public ReporteController(IWebHostEnvironment env)
        {
            if (!ventas.Any())
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "TrackingComercial", "facturas.json");

                if (System.IO.File.Exists(path))
                {
                    var json = System.IO.File.ReadAllText(path);
                    ventas = string.IsNullOrWhiteSpace(json)
                        ? new List<FacturaDTO>()
                        : JsonSerializer.Deserialize<List<FacturaDTO>>(json, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        }) ?? new List<FacturaDTO>();

                    clientes = ventas
                        .Select(v => v.Cliente)
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();
                }
            }
        }

        [HttpGet]
        public IActionResult Index()
        {
            var usuario = HttpContext.Session.GetString("UsuarioTrackingComercial");
            if (string.IsNullOrEmpty(usuario))
            {
                return RedirectToAction("Login", "Home");
            }
            ViewBag.Clientes = new SelectList(clientes);
            return View();
        }

        [HttpPost]
        public IActionResult Index(string cliente)
        {
            var usuario = HttpContext.Session.GetString("UsuarioTrackingComercial");
            if (string.IsNullOrEmpty(usuario))
            {
                return RedirectToAction("Login", "Home");
            }

            int anioActual = DateTime.Now.Year;
            int mesActual = DateTime.Now.Month;
            int anioAnterior = anioActual - 1;
            int mesAnterior = mesActual > 1 ? mesActual - 1 : 12;

            var tabla = new List<ReporteDTO>();

            foreach (var producto in productos)
            {
                decimal ventasAnt = SumarVentas(cliente, producto, anioAnterior, mesAnterior);
                decimal ventasAct = SumarVentas(cliente, producto, anioActual, mesAnterior);

                decimal porcentaje;
                string color;

                if (ventasAnt == 0 && ventasAct > 0)
                {
                    porcentaje = 999;
                    color = "verde";
                }
                else if (ventasAnt == 0)
                {
                    porcentaje = 0;
                    color = "negro";
                }
                else
                {
                    porcentaje = Math.Round(((ventasAct / ventasAnt) - 1) * 100, 2);
                    color = porcentaje > 0 ? "verde" : porcentaje < 0 ? "rojo" : "negro";
                }

                tabla.Add(new ReporteDTO
                {
                    Producto = producto,
                    VentasAnioAnterior = ventasAnt,
                    VentasAnioActual = ventasAct,
                    Porcentaje = porcentaje,
                    Color = color
                });
            }

            var totalAnt = tabla.Sum(t => t.VentasAnioAnterior);
            var totalAct = tabla.Sum(t => t.VentasAnioActual);

            decimal porcentajeTotal;
            string colorTotal;

            if (totalAnt == 0 && totalAct > 0)
            {
                porcentajeTotal = 999;
                colorTotal = "verde";
            }
            else if (totalAnt == 0)
            {
                porcentajeTotal = 0;
                colorTotal = "negro";
            }
            else
            {
                porcentajeTotal = Math.Round(((totalAct / totalAnt) - 1) * 100, 2);
                colorTotal = porcentajeTotal > 0 ? "verde" : porcentajeTotal < 0 ? "rojo" : "negro";
            }

            tabla.Insert(0, new ReporteDTO
            {
                Producto = "<b>TOTALES</b>",
                VentasAnioAnterior = totalAnt,
                VentasAnioActual = totalAct,
                Porcentaje = porcentajeTotal,
                Color = colorTotal
            });

            ViewBag.Clientes = new SelectList(clientes);
            ViewBag.ClienteSeleccionado = cliente;

            return View(tabla);
        }

        private decimal SumarVentas(string cliente, string producto, int anio, int mes)
        {
            var mesesValidos = Enumerable.Range(1, mes).Select(m => $"{anio} {m:D2}").ToList();

            return ventas
                .Where(v => v.Cliente == cliente && v.Producto == producto && mesesValidos.Contains(v.AnioMes))
                .Sum(v => v.Venta);
        }
    }
}
