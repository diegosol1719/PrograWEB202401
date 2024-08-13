using System;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Khareedo.Models;
using PagedList;

namespace IMS_Project.Controllers
{
    public class CategoriesController : Controller
    {
        private Entities db = new Entities();

        // GET: Categories      
        public ActionResult Index(string searchString, int? page)
        {
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            var categories = db.Categories.OrderBy(c => c.Name).ToList();

            // Aplicar la búsqueda si hay un término de búsqueda
            if (!String.IsNullOrEmpty(searchString))
            {
                categories = categories.Where(c => c.Name.Contains(searchString)).ToList();
            }

            // Convertir a PagedList para la paginación
            var pagedCategories = categories.ToPagedList(pageNumber, pageSize);

            // Pasar el término de búsqueda a la vista
            ViewBag.SearchString = searchString;

            return View("~/Views/ViewsA/Categories/Index.cshtml", pagedCategories);
        }

        // GET: Categories/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Categories category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Categories/Details.cshtml", category);
        }

        // GET: Categories/Create
        public ActionResult Create()
        {
            return View("~/Views/ViewsA/Categories/Create.cshtml");
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryID,Name,Description,Picture1,Picture2,isActive")] Categories category)
        {
            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View("~/Views/ViewsA/Categories/Create.cshtml", category);
        }

        // GET: Categories/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Categories category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Categories/Edit.cshtml", category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,Name,Description,Picture1,Picture2,isActive")] Categories category)
        {
            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("~/Views/ViewsA/Categories/Edit.cshtml", category);
        }

        // GET: Categories/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Categories category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Categories/Delete.cshtml", category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Categories category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            db.Categories.Remove(category);
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
