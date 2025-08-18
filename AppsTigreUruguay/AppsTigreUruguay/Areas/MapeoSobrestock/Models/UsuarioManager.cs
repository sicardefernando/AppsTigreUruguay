using System.Text.Json;

namespace AppsTigreUruguay.Areas.MapeoSobrestock.Models
{
    public static class UsuarioManager
    {
        private static string Ruta => Path.Combine(Directory.GetCurrentDirectory(), "Data", "MapeoSobrestock", "usuarios.json");

        public static List<Usuario> ObtenerUsuarios()
        {
            if (!File.Exists(Ruta))
                return new List<Usuario>();

            string json = File.ReadAllText(Ruta);
            return JsonSerializer.Deserialize<List<Usuario>>(json) ?? new List<Usuario>();
        }

        public static bool ValidarLogin(string usuario, string clave)
        {
            return ObtenerUsuarios().Any(u => u.UsuarioNombre == usuario && u.Clave == clave);
        }

        public static bool UsuarioExiste(string usuario)
        {
            return ObtenerUsuarios().Any(u => u.UsuarioNombre == usuario);
        }

        public static void AgregarUsuario(string usuario, string clave)
        {
            var usuarios = ObtenerUsuarios();
            usuarios.Add(new Usuario { UsuarioNombre = usuario, Clave = clave });

            var carpeta = Path.GetDirectoryName(Ruta);
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            var opciones = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(Ruta, JsonSerializer.Serialize(usuarios, opciones));
        }
    }
}
