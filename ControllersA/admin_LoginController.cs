using System.Linq;
using System.Web.Mvc;
using Khareedo.Models;

namespace Khareedo.Controllers
{
    public class admin_LoginController : Controller
    {
        Entities db = new Entities();

        // GET: admin_Login
        public ActionResult Index()
        {
            return View("~/Views/admin_Login/Index.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(AdminUsers login)
        {
            if (ModelState.IsValid)
            {
                var user = db.AdminUsers.FirstOrDefault(m => m.UserName == login.UserName && m.Password == login.Password);

                if (user != null)
                {
                    Session["username"] = user.UserName; // Guardar el nombre de usuario en la sesión
                    Session["EmpID"] = user.UserID;      // Guardar el ID del usuario en la sesión
                    Session["RoleType"] = user.RoleType;      // Guardar el ID del usuario en la sesión

                    return RedirectToAction("Index", "Dashboard"); // Redirigir al dashboard u otra página de destino
                }
                else
                {
                    ModelState.AddModelError("", "Usuario o contraseña incorrectos."); // Agregar un error personalizado al modelo
                }
            }
            return View("Index", login); // Volver a la vista de login con los datos para mostrar el error
        }




        public ActionResult Logout()
        {
            Session.Clear(); // Limpiar la sesión
            Session.Abandon(); // Abandonar la sesión
            return RedirectToAction("Index", "admin_Login"); // Redirigir al inicio de sesión después del cierre de sesión
        }
    }
}

