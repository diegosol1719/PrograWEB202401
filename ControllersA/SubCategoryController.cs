using System;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using IMS_Project.Models;
using Khareedo.Models;
using PagedList;


namespace IMS_Project.Controllers
{
    public class SubCategoryController : Controller
    {
        private Entities db = new Entities();

        // GET: SubCategory


        public ActionResult Index(string searchString, int? page)
        {
            int pageSize = 10; // Número de elementos por página
            int pageNumber = (page ?? 1); // Página actual (por defecto 1)

            var subcategories = db.SubCategories.OrderBy(s => s.Name).ToList();

            // Aplicar la búsqueda si hay un término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                subcategories = subcategories.Where(s => s.Name.Contains(searchString)).ToList();
            }

            // Convertir la lista filtrada en un objeto PagedList para la paginación
            var pagedSubcategories = subcategories.ToPagedList(pageNumber, pageSize);

            // Pasar el término de búsqueda de vuelta a la vista para mantener el estado
            ViewBag.SearchString = searchString;

            return View("~/Views/ViewsA/SubCategory/Index.cshtml", pagedSubcategories);
        }



        // GET: SubCategory/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubCategory subCategory = db.SubCategories.Find(id);
            if (subCategory == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/SubCategory/Details.cshtml", subCategory);
        }

        // GET: SubCategory/Create
        public ActionResult Create()
        {
            return View("~/Views/ViewsA/SubCategory/Create.cshtml");
        }

        // POST: SubCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "SubCategoryID,CategoryID,Name,Description,Picture1,Picture2,isActive")] SubCategory subCategory)
        {
            if (ModelState.IsValid)
            {
                subCategory.CategoryID = 1; // Establecer CategoryID como 1

                // Aquí asumes que tienes una instancia de tu contexto de base de datos `db` como `Entities`
                db.SubCategories.Add(subCategory);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View("~/Views/ViewsA/SubCategory/Create.cshtml", subCategory);
        }

        // GET: SubCategory/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            SubCategory subCategory = db.SubCategories.Find(id);
            if (subCategory == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/SubCategory/Edit.cshtml", subCategory);
        }

        // POST: SubCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "SubCategoryID,CategoryID,Name,Description,Picture1,Picture2,isActive")] SubCategory subCategory)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    subCategory.CategoryID = 1; // Establecer CategoryID como 1

                    // Aquí asumes que tienes una instancia de tu contexto de base de datos `db` como `Entities`
                    db.Entry(subCategory).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (DbEntityValidationException ex)
            {
                // Manejo de errores de validación de entidad
                foreach (var validationError in ex.EntityValidationErrors)
                {
                    foreach (var error in validationError.ValidationErrors)
                    {
                        ModelState.AddModelError("", $"{error.PropertyName}: {error.ErrorMessage}");
                    }
                }
            }

            return View("~/Views/ViewsA/SubCategory/Edit.cshtml", subCategory);
        }

        // GET: SubCategory/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            SubCategory subCategory = db.SubCategories.Find(id);
            if (subCategory == null)
            {
                return HttpNotFound();
            }

            return View("~/Views/ViewsA/SubCategory/Delete.cshtml", subCategory);
        }

        // POST: SubCategory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            SubCategory subCategory = db.SubCategories.Find(id);
            if (subCategory == null)
            {
                return HttpNotFound();
            }

            db.SubCategories.Remove(subCategory);
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
