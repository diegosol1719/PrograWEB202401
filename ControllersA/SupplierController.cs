using Khareedo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using PagedList;
using PagedList.Mvc;


namespace IMS_Project.Controllers
{
    public class SupplierController : Controller
    {
        Entities db = new Entities();

        // GET: Supplier
        public ActionResult Index(string searchString, int? page)
        {
            var suppliers = db.Suppliers.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                suppliers = suppliers.Where(s => s.CompanyName.Contains(searchString));
            }

            int pageSize = 10; 
            int pageNumber = (page ?? 1); // Si pageNumber tiene valor, usa ese; de lo contrario, usa 1 como valor predeterminado

            // Devolver los proveedores paginados a la vista
            return View("~/Views/ViewsA/Supplier/Index.cshtml", suppliers.OrderBy(s => s.SupplierID).ToPagedList(pageNumber, pageSize));
        }

        public ActionResult Create()
        {
            return View("~/Views/ViewsA/Supplier/Create.cshtml");
        }

        [HttpPost]
        public ActionResult Create(Suppliers supp)
        {
            if (ModelState.IsValid)
            {
                db.Suppliers.Add(supp);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("~/Views/ViewsA/Supplier/Create.cshtml");
        }

        public ActionResult Edit(int id)
        {
            Suppliers supp = db.Suppliers.Single(x => x.SupplierID == id);
            if (supp == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Supplier/Edit.cshtml", supp);
        }

        [HttpPost]
        public ActionResult Edit(Suppliers supp)
        {
            if (ModelState.IsValid)
            {
                db.Entry(supp).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View("~/Views/ViewsA/Supplier/Edit.cshtml", supp);
        }

        public ActionResult Details(int id)
        {
            Suppliers supp = db.Suppliers.Find(id);
            if (supp == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Supplier/Details.cshtml", supp);
        }

        public ActionResult Delete(int id)
        {
            Suppliers supp = db.Suppliers.Find(id);
            if (supp == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Supplier/Delete.cshtml", supp);
        }

        //Post Delete Confirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Suppliers supp = db.Suppliers.Find(id);
            db.Suppliers.Remove(supp);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
