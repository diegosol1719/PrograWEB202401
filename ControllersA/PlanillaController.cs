using System;
using System.Linq;
using System.Security.Claims;
using System.Web.Mvc;
using Khareedo.Models;
using PagedList;

namespace Khareedo.ControllersA
{
    public class PlanillaController : Controller
    {
        private readonly Entities db = new Entities();
        private int GetUserRole()
        {
            if (Session["RoleType"] != null)
            {
                return (int)Session["RoleType"];
            }
            return 0; // Valor predeterminado si no se encuentra el rol
        }
        public ActionResult Index(string searchString, int? page)
        {
            var userRole = GetUserRole();
            TempData["UserRole"] = userRole;

            // Configuración de la paginación
            int pageSize = 10; // Número de elementos por página
            int pageNumber = page ?? 1; // Número de página actual

            IQueryable<Incapacidades> incapacidadesQuery = db.Incapacidades.Include("AdminUsers"); // Incluye la información del usuario asociado

            if (userRole == 0) // Si el rol es 0, filtra solo las incapacidades del usuario logueado
            {
                var userId = (int)Session["EmpID"]; // Obtén el ID del usuario desde la sesión
                incapacidadesQuery = incapacidadesQuery.Where(i => i.UserID == userId);
            }

            // Filtra por nombre si se proporciona una cadena de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                incapacidadesQuery = incapacidadesQuery
                    .Where(i => (i.AdminUsers.FirstName + " " + i.AdminUsers.LastName).Contains(searchString));
            }

            // Aplica paginación y ordena por ID de incapacidad
            var incapacidades = incapacidadesQuery
                .OrderBy(i => i.IncapacidadID)
                .ToPagedList(pageNumber, pageSize);

            // Guarda la cadena de búsqueda en ViewBag para usarla en la vista
            ViewBag.SearchString = searchString;

            return View("~/Views/ViewsA/Planilla/Index.cshtml", incapacidades);
        }



        // Agregar Incapacidad
        public ActionResult Create()
        {
            var userID = Session["EmpID"] as int?;

            if (userID == null)
            {
                ModelState.AddModelError("", "No se pudo obtener el ID del usuario.");

                // Obtener la lista de usuarios y convertir en memoria
                var users = db.AdminUsers
                              .ToList() // Obtener los datos en memoria
                              .Select(u => new SelectListItem
                              {
                                  Value = u.UserID.ToString(), // Convertir UserID a string en memoria
                                  Text = $"ID-{u.UserID} {u.FirstName} {u.LastName}" // Formato: ID-UserID Nombre Apellido
                              })
                              .ToList();

                ViewBag.Users = users;

                return View("~/Views/ViewsA/Planilla/Create.cshtml"); // Especifica la ruta completa de la vista
            }

            var model = new Incapacidades
            {
                UserID = userID.Value,
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now,
                Motivo = string.Empty // Inicializa Motivo como una cadena vacía o un valor predeterminado
            };

            // Obtener la lista de usuarios para el DropDownList
            var allUsers = db.AdminUsers
                              .ToList() // Obtener los datos en memoria
                              .Select(u => new SelectListItem
                              {
                                  Value = u.UserID.ToString(),
                                  Text = $"ID-{u.UserID} {u.FirstName} {u.LastName}"
                              })
                              .ToList();

            ViewBag.Users = allUsers;

            return View("~/Views/ViewsA/Planilla/Create.cshtml", model); // Especifica la ruta completa de la vista
        }

        // POST: Planilla/AgregarIncapacidad
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AgregarIncapacidad(Incapacidades model)
        {
            if (ModelState.IsValid)
            {
                db.Incapacidades.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index", "Planilla");
            }

            // Volver a establecer la lista de usuarios si hay un error de validación
            var allUsers = db.AdminUsers
                              .ToList() // Obtener los datos en memoria
                              .Select(u => new SelectListItem
                              {
                                  Value = u.UserID.ToString(),
                                  Text = $"ID-{u.UserID} {u.FirstName} {u.LastName}"
                              })
                              .ToList();

            ViewBag.Users = allUsers;

            return View("~/Views/ViewsA/Planilla/Create.cshtml", model); // Especifica la ruta completa de la vista
        }

        // Registrar Asistencia
        [HttpPost]
        public ActionResult RegistrarAsistencia(int userID, DateTime fecha, string estado, TimeSpan? horaEntrada = null, TimeSpan? horaSalida = null)
        {
            var asistencia = new Asistencia
            {
                UserID = userID,
                Fecha = fecha,
                Estado = estado,
                HoraEntrada = horaEntrada,
                HoraSalida = horaSalida
            };

            db.Asistencia.Add(asistencia);
            db.SaveChanges();
            return RedirectToAction("Index"); // Redirigir a una vista principal
        }

        // Método para calcular rebajos automáticos (ejemplo simple)
        public decimal CalcularRebajosAutomaticos(int userID)
        {
            decimal totalRebajos = 0;

            var tardanzas = db.Asistencia
                .Where(a => a.UserID == userID && a.Estado == "Tarde")
                .Count();

            totalRebajos += tardanzas * 5; // Supongamos que cada tardanza tiene un rebajo de 5 unidades

            return totalRebajos;
        }

        // GET: Planilla/Delete/5
        public ActionResult Delete(int id)
        {
            var incapacidad = db.Incapacidades.Find(id);
            if (incapacidad == null)
            {
                return HttpNotFound();
            }

            // Obtén el rol del usuario actual
            var userRole = User.IsInRole("Admin") ? 1 : 0;

            ViewBag.UserRole = userRole; // Pasa el rol a la vista
            return View("~/Views/ViewsA/Planilla/Delete.cshtml", incapacidad); // Especifica la ruta completa de la vista
        }

        // POST: Planilla/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var incapacidad = db.Incapacidades.Find(id);
            if (incapacidad == null)
            {
                return HttpNotFound();
            }

            db.Incapacidades.Remove(incapacidad);
            db.SaveChanges();

            return RedirectToAction("Index"); 
        }
    }
}
