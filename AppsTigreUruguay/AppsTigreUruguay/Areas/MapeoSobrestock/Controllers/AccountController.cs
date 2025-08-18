using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using AppsTigreUruguay.Areas.MapeoSobrestock.Models;

namespace AppsTigreUruguay.Areas.MapeoSobrestock.Controllers
{
    [Area("MapeoSobrestock")]
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string usuario, string clave)
        {
            // Normalizamos a minúsculas
            var usuarioNormalizado = usuario?.Trim().ToLowerInvariant();

            if (UsuarioManager.ValidarLogin(usuarioNormalizado, clave))
            {
                HttpContext.Session.SetString("UsuarioMapeoSobrestock", usuarioNormalizado);
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Usuario o contraseña incorrectos.";
            return View();
        }

        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(string usuario, string clave)
        {
            // Normalizamos a minúsculas
            var usuarioNormalizado = usuario?.Trim().ToLowerInvariant();

            if (UsuarioManager.UsuarioExiste(usuarioNormalizado))
            {
                ViewBag.Error = "El usuario ya existe.";
                return View();
            }

            UsuarioManager.AgregarUsuario(usuarioNormalizado, clave);
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
