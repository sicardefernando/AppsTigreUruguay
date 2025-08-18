using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using AppsTigreUruguay.Areas.TrackingComercial.Models;

namespace AppsTigreUruguay.Areas.TrackingComercial.Controllers
{
    [Area("TrackingComercial")]
    public class HomeController : Controller
    {
        private const string CLAVE_ADMIN = "adminTC135";
        private readonly string rutaUsuarios = Path.Combine(Directory.GetCurrentDirectory(), "Data", "TrackingComercial", "usuarios.json");

        [HttpGet]
        public IActionResult Index()
        {
            var usuario = HttpContext.Session.GetString("UsuarioTrackingComercial");
            if (string.IsNullOrEmpty(usuario))
            {
                return RedirectToAction("Login");
            }

            ViewBag.Usuario = usuario;
            return RedirectToAction("Index", "Reporte");
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string usuario, string clave)
        {
            // Ruta al archivo usuarios.json
            var rutaJson = Path.Combine(Directory.GetCurrentDirectory(), "Data", "TrackingComercial", "usuarios.json");

            if (!System.IO.File.Exists(rutaJson))
            {
                ViewBag.Error = "No se encontró el archivo de usuarios.";
                return View();
            }

            var json = System.IO.File.ReadAllText(rutaJson);

            List<Usuario>? usuarios;
            try
            {
                usuarios = JsonSerializer.Deserialize<List<Usuario>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch
            {
                ViewBag.Error = "Error al leer los usuarios.";
                return View();
            }

            var encontrado = usuarios?.FirstOrDefault(u => u.UsuarioNombre == usuario && u.Clave == clave);

            if (encontrado != null)
            {
                HttpContext.Session.SetString("UsuarioTrackingComercial", usuario);
                return RedirectToAction("Index", "Reporte");
            }

            ViewBag.Error = "Usuario o clave incorrectos.";
            return View();
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string usuario, string clave, string confirmarClave, string claveAdmin)
        {
            if (claveAdmin != CLAVE_ADMIN)
            {
                ViewBag.Error = "Clave de administrador incorrecta.";
                return View();
            }

            if (clave != confirmarClave)
            {
                ViewBag.Error = "Las contraseñas no coinciden.";
                return View();
            }

            List<Usuario> usuarios = new();

            if (System.IO.File.Exists(rutaUsuarios))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(rutaUsuarios);
                    usuarios = JsonSerializer.Deserialize<List<Usuario>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<Usuario>();
                }
                catch
                {
                    ViewBag.Error = "Error al leer el archivo de usuarios.";
                    return View();
                }
            }

            if (usuarios.Any(u => u.UsuarioNombre == usuario))
            {
                ViewBag.Error = "El usuario ya existe.";
                return View();
            }

            usuarios.Add(new Usuario { UsuarioNombre = usuario, Clave = clave });

            try
            {
                var nuevoJson = JsonSerializer.Serialize(usuarios, new JsonSerializerOptions { WriteIndented = true });
                System.IO.File.WriteAllText(rutaUsuarios, nuevoJson);
            }
            catch
            {
                ViewBag.Error = "Error al guardar el nuevo usuario.";
                return View();
            }

            ViewBag.Mensaje = "Usuario registrado con éxito. Ahora puede iniciar sesión.";
            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
