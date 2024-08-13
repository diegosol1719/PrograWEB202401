using System;
using System.Linq;
using System.Web.Mvc;
using Khareedo.Models;
using PagedList;

namespace Khareedo.Controllers
{
    public class AsistenciaController : Controller
    {
        private Entities db = new Entities();

        public ActionResult Create()
        {
            var userID = Session["EmpID"] as int?;

            if (userID == null)
            {
                ModelState.AddModelError("", "No se pudo obtener el ID del usuario.");
                return View();
            }

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

            var model = new Asistencia
            {
                UserID = userID.Value,
                Fecha = DateTime.Now,
                Estado = string.Empty,
                HoraEntrada = null,
                HoraSalida = null
            };

            return View("~/Views/ViewsA/Asistencia/Create.cshtml", model);
        }

        // POST: Asistencia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Asistencia model)
        {
            var userID = Session["EmpID"] as int?;

            if (ModelState.IsValid)
            {
                db.Asistencia.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index", "Asistencia");
            }

            // Volver a establecer la lista de usuarios si hay un error de validación
            ViewBag.Users = db.AdminUsers
                              .ToList() // Obtener los datos en memoria
                              .Select(u => new SelectListItem
                              {
                                  Value = u.UserID.ToString(), // Convertir UserID a string en memoria
                                  Text = $"ID-{u.UserID} {u.FirstName} {u.LastName}" // Formato: ID-UserID Nombre Apellido
                              })
                              .ToList();

            return View("~/Views/ViewsA/Asistencia/Create.cshtml", model);
        }



        // GET: Asistencia/Delete/5
        public ActionResult Delete(int id)
        {
            var asistencia = db.Asistencia.Find(id);
            if (asistencia == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Asistencia/Delete.cshtml", asistencia);
        }

        // POST: Asistencia/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var asistencia = db.Asistencia.Find(id);
            if (asistencia == null)
            {
                return HttpNotFound();
            }
            db.Asistencia.Remove(asistencia);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        // GET: Asistencia/Index

        public ActionResult Index(string searchString, int? page)
        {
            var userRole = GetUserRole(); // Obtén el rol del usuario desde la sesión
            TempData["UserRole"] = userRole; // Almacena el rol en TempData

            // Configuración de la paginación
            int pageSize = 10; // Número de elementos por página
            int pageNumber = page ?? 1; // Número de página actual

            IQueryable<Asistencia> asistenciasQuery = db.Asistencia; // Obtén todas las asistencias

            if (userRole == 0) // Si el rol es 0, filtra solo las asistencias del usuario logueado
            {
                var userId = (int)Session["EmpID"]; // Obtén el ID del usuario desde la sesión
                asistenciasQuery = asistenciasQuery.Where(a => a.UserID == userId);
            }

            // Filtra por nombre si se proporciona una cadena de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                asistenciasQuery = asistenciasQuery
                    .Where(a => (a.AdminUsers != null &&
                                 (a.AdminUsers.FirstName + " " + a.AdminUsers.LastName).Contains(searchString)));
            }

            // Aplica paginación y ordena por ID de asistencia
            var asistencias = asistenciasQuery
                .OrderBy(a => a.AsistenciaID)
                .ToPagedList(pageNumber, pageSize);

            return View("~/Views/ViewsA/Asistencia/Index.cshtml", asistencias);
        }



        private int GetUserRole()
        {
            if (Session["RoleType"] != null)
            {
                return (int)Session["RoleType"];
            }
            return 0; // Valor predeterminado si no se encuentra el rol
        }




    }
}


