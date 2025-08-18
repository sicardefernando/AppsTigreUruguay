using System.Text.Json;

namespace AppsTigreUruguay.Areas.MapeoSobrestock.Models
{
    public static class LoggerMovimientos
    {
        private static string Ruta => Path.Combine(Directory.GetCurrentDirectory(), "Data", "MapeoSobrestock", "movimientos.json");

        public static void RegistrarMovimiento(string usuario, string accion, string detalle)
        {
            var carpeta = Path.GetDirectoryName(Ruta);
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            List<MovimientoUsuario> movimientos;
            if (File.Exists(Ruta))
            {
                var contenido = File.ReadAllText(Ruta);
                movimientos = JsonSerializer.Deserialize<List<MovimientoUsuario>>(contenido) ?? new List<MovimientoUsuario>();
            }
            else
            {
                movimientos = new List<MovimientoUsuario>();
            }

            movimientos.Add(new MovimientoUsuario
            {
                Usuario = usuario,
                Accion = accion,
                FechaHora = DateTime.Now,
                Detalle = detalle
            });

            var opciones = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(Ruta, JsonSerializer.Serialize(movimientos, opciones));
        }
    }
}
