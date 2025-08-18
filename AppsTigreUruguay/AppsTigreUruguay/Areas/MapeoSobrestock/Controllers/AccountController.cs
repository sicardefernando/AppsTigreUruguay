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
            if (UsuarioManager.ValidarLogin(usuario, clave))
            {
                HttpContext.Session.SetString("UsuarioMapeoSobrestock", usuario);
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
            if (UsuarioManager.UsuarioExiste(usuario))
            {
                ViewBag.Error = "El usuario ya existe.";
                return View();
            }

            UsuarioManager.AgregarUsuario(usuario, clave);
            return RedirectToAction("Login");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
