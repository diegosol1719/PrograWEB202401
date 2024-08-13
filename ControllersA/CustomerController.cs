using Khareedo.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using PagedList.Mvc;


namespace IMS_Project.Controllers
{
    public class CustomerController : Controller
    {
        Entities db = new Entities();

        public ActionResult Index(string searchString, int? page)
        {
            var customers = db.Customers.AsQueryable();

            // Filtrar por cadena de búsqueda si se proporciona
            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c => c.First_Name.Contains(searchString));
            }

            // Número de elementos por página
            int pageSize = 10;
            // Número de página actual
            int pageNumber = (page ?? 1);

            // Obtener la lista de clientes paginados
            var pagedCustomers = customers.OrderBy(c => c.CustomerID).ToPagedList(pageNumber, pageSize);

            // Pasar la lista paginada a la vista
            return View("~/Views/ViewsA/Customer/Index.cshtml", pagedCustomers);
        }


        public ActionResult Create()
        {
            return View("~/Views/ViewsA/Customer/Create.cshtml");
        }

        [HttpPost]
        public ActionResult Create(Customers customer)
        {
            if (ModelState.IsValid)
            {
                var existingCustomer = db.Customers.FirstOrDefault(c => c.Email == customer.Email);

                if (existingCustomer != null)
                {
                    ModelState.AddModelError("Email", "El correo electrónico ya está registrado.");
                    return View("~/Views/ViewsA/Customer/Create.cshtml", customer);
                }

                db.Customers.Add(customer);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            // Si el modelo no es válido, vuelve a mostrar el formulario con los datos ingresados
            return View("~/Views/ViewsA/Customer/Create.cshtml", customer);
        }


        //Get Edit
        [HttpGet]
        public ActionResult Edit(int id)
        {
            Customers cust = db.Customers.Single(x => x.CustomerID == id);
            if (cust == null)
            {
                return HttpNotFound();
            }

            return View("~/Views/ViewsA/Customer/Edit.cshtml", cust);
        }

        //Post Edit
        [HttpPost]
        public ActionResult Edit(Customers cust)
        {
            if (ModelState.IsValid)
            {
                db.Entry(cust).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View("~/Views/ViewsA/Customer/Edit.cshtml", cust);
        }
        //Get Details
        public ActionResult Details(int id)
        {
            Customers cust = db.Customers.Find(id);
            if (cust == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Customer/Details.cshtml", cust);
        }

        //Get Delete
        public ActionResult Delete(int id)
        {
            Customers cust = db.Customers.Find(id);

            if (cust == null)
            {
                return HttpNotFound();
            }

            return View("~/Views/ViewsA/Customer/Delete.cshtml", cust);
        }

        //Post Delete Confirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Customers cust = db.Customers.Find(id);
            db.Customers.Remove(cust);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
