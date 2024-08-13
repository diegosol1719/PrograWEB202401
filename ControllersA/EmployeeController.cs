using Khareedo.Models;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using PagedList; 
using System.Web.Mvc;
using System.Collections.Generic;
using System.IO;
using System;

namespace IMS_Project.Controllers
{
    public class EmployeeController : Controller
    {
        private Entities db = new Entities();

        public ActionResult Index(string searchString, int? page)
        {
            int pageSize = 10; // Número de registros por página
            int pageNumber = (page ?? 1); // Página actual (por defecto la primera)

            var employees = db.AdminUsers.AsQueryable();

            // Filtrar por nombre si se proporciona un término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e => e.FirstName.Contains(searchString) || e.LastName.Contains(searchString));
                ViewBag.SearchString = searchString; // Guardar el término de búsqueda para mantenerlo en la vista
            }

            // Ordenar por algún criterio si es necesario
            employees = employees.OrderBy(e => e.LastName).ThenBy(e => e.FirstName);

            // Aplicar paginación usando la biblioteca PagedList.Mvc
            var pagedEmployees = employees.ToPagedList(pageNumber, pageSize);

            return View("~/Views/ViewsA/Employee/Index.cshtml", pagedEmployees);
        }

        // GET: Employee/Create
        public ActionResult Create()
        {
            var userRole = GetUserRole(); // Obtén el rol del usuario desde la sesión
            ViewBag.UserRole = userRole;

            return View("~/Views/ViewsA/Employee/Create.cshtml");
        }

        private int GetUserRole()
        {
            if (Session["RoleType"] != null)
            {
                return (int)Session["RoleType"];
            }
            return 0; // Valor predeterminado si no se encuentra el rol
        }


        // POST: Employee/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(AdminUsers combinedAdmin)
        {
            if (ModelState.IsValid)
            {
                // Verificar si el correo electrónico ya está registrado
                var existingAdmin = db.AdminUsers.FirstOrDefault(a => a.Email == combinedAdmin.Email);
                if (existingAdmin != null)
                {
                    ModelState.AddModelError("Email", "El correo electrónico ya está registrado.");
                    TempData["Phone"] = combinedAdmin.Phone; // Guardar el teléfono en TempData
                    TempData["Password"] = combinedAdmin.Password; // Guardar la contraseña en TempData
                    return View("~/Views/ViewsA/Employee/Create.cshtml", combinedAdmin);
                }

                try
                {
                    db.AdminUsers.Add(combinedAdmin);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
            }
            else
            {
                // Manejar errores de validación personalizados aquí
                ModelState.AddModelError("", "Por favor, corrija los errores indicados.");
            }

            // Guardar valores de teléfono y contraseña en TempData
            TempData["Phone"] = combinedAdmin.Phone;
            TempData["Password"] = combinedAdmin.Password;

            // Devolver la vista con el modelo y los errores
            return View("~/Views/ViewsA/Employee/Create.cshtml", combinedAdmin);
        }



        // GET: Employee/Edit/5
        public ActionResult Edit(int id)
        {
            var userRole = GetUserRole(); // Obtén el rol del usuario desde la sesión
            ViewBag.UserRole = userRole;
            AdminUsers combined_Admin = db.AdminUsers.Find(id);
            if (combined_Admin == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Employee/Edit.cshtml", combined_Admin);
        }

        // POST: Employee/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AdminUsers combined_Admin)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    db.Entry(combined_Admin).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (DbEntityValidationException ex)
                {
                    // Capturar y mostrar errores de validación
                    foreach (var validationErrors in ex.EntityValidationErrors)
                    {
                        foreach (var validationError in validationErrors.ValidationErrors)
                        {
                            ModelState.AddModelError(validationError.PropertyName, validationError.ErrorMessage);
                        }
                    }
                }
            }
            // Si llegamos aquí, significa que hubo errores de validación, volvemos a la vista con el modelo para corregir
            return View("~/Views/ViewsA/Employee/Edit.cshtml", combined_Admin);
        }


        // GET: Employee/Details/5
        public ActionResult Details(int id)
        {
            var combined_Admin = db.AdminUsers.Find(id);
            if (combined_Admin == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Employee/Details.cshtml", combined_Admin);
        }


        // GET: Employee/Delete/5
        public ActionResult Delete(int id)
        {
            AdminUsers combined_Admin = db.AdminUsers.Find(id);
            if (combined_Admin == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Employee/Delete.cshtml", combined_Admin);
        }

        // POST: Employee/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            AdminUsers combined_Admin = db.AdminUsers.Find(id);
            db.AdminUsers.Remove(combined_Admin);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }



   

    }
}
