using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reporteadores.Models;

namespace Reporteadores.Controllers
{
    public class AccountController : Controller
    {
        private ComparteContext db = new ComparteContext();

        //private ILoginServices _loginService;
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

        public AccountController(UserManager<Usuario> userManager, SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        public IActionResult Login(Usuario model)
        {
            try
            {
                var username = HttpContext.Session.GetString("Username"); // Recupera el nombre de usuario de la sesión

                if (username != null)
                {
                    // El usuario está autenticado, realiza la lógica deseada
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    var usuario = db.Usuarios.Where(u => u.UsCorto == model.UsCorto)
                                            .Select(u => new { u.UsCorto, u.UsPasswrd })  // Solo selecciona 'UsCorto'
                                            .FirstOrDefault();

                    if (usuario != null && usuario.UsPasswrd == model.UsPasswrd)
                    {
                        HttpContext.Session.SetString("Username", model.UsCorto); // Almacena el nombre de usuario en la sesión

                        _userManager.UpdateAsync(model);
                        _signInManager.RefreshSignInAsync(model);

                        // Autenticación exitosa, redirigir al usuario a la página principal o a otra página deseada
                        
                        return RedirectToAction("Index", "Home");
                    }

                }
                // Verificar si el usuario existe en la base de datos 379911307367103

            }

            catch
            {
                return View();
            }
            return View();
        }
    }
}
