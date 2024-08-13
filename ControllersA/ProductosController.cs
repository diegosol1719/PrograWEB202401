using System;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Khareedo.Models;
using PagedList;
using PagedList.Mvc;

namespace Khareedo.Controllers
{
    public class ProductosController : Controller
    {
        private Entities db = new Entities();

        // GET: Productos
        public ActionResult Index(string searchString, int? page)
        {
            var products = db.Products.Include(p => p.Categories).Include(p => p.SubCategory).Include(p => p.Suppliers);

            // Filtrar por cadena de búsqueda si se proporciona
            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            // Número de elementos por página
            int pageSize = 10;
            // Número de página actual
            int pageNumber = (page ?? 1);

            // Obtener la lista de productos paginados
            var pagedProducts = products.OrderBy(p => p.ProductID).ToPagedList(pageNumber, pageSize);

            // Pasar la lista paginada a la vista
            return View(pagedProducts);
        }


        // GET: Productos/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Productos/Create
        // GET: Productos/Create
      public ActionResult Create()
{
    ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name");
    ViewBag.SubCategoryID = new SelectList(db.SubCategories, "SubCategoryID", "Name");
    ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName");

    // Aquí puedes inicializar otras propiedades según sea necesario
    
    return View();
}


        // POST: Productos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ProductID,Name,SupplierID,CategoryID,SubCategoryID,QuantityPerUnit,UnitPrice,OldPrice,UnitWeight,Size,Discount,UnitInStock,UnitOnOrder,ProductAvailable,ImageURL,AltText,AddBadge,OfferTitle,OfferBadgeClass,ShortDescription,LongDescription,Picture1,Picture2,Picture3,Picture4,Note")] Products product, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);
                        string path = Path.Combine(Server.MapPath("~/Uploads"), fileName);

                        // Guarda la imagen en la carpeta del servidor
                        imageFile.SaveAs(path);

                        // Guarda la URL de la imagen en la base de datos
                        product.ImageURL = "/Uploads/" + fileName;
                    }

                    db.Products.Add(product);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Error al crear producto: " + ex.Message;
                    // Manejo de errores
                }
            }

            // Si el modelo no es válido, vuelve a cargar las listas necesarias para las SelectList
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name", product.CategoryID);
            ViewBag.SubCategoryID = new SelectList(db.SubCategories, "SubCategoryID", "Name", product.SubCategoryID);
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName", product.SupplierID);
            return View(product);
        }




        // GET: Productos/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Products product = db.Products.Find(id);

            if (product == null)
            {
                return HttpNotFound();
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "Name", product.CategoryID);
            ViewBag.SubCategoryID = new SelectList(db.SubCategories, "SubCategoryID", "Name", product.SubCategoryID);
            ViewBag.SupplierID = new SelectList(db.Suppliers, "SupplierID", "CompanyName", product.SupplierID);

            return View(product);
        }

        // POST: Productos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Products product, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    using (var db = new Entities())
                    {
                        var existingProduct = db.Products.Find(product.ProductID);
                        if (existingProduct != null)
                        {
                            if (imageFile != null && imageFile.ContentLength > 0)
                            {
                                // Eliminar la imagen anterior si existe
                                if (!string.IsNullOrEmpty(existingProduct.ImageURL))
                                {
                                    string imagePath = Server.MapPath("~" + existingProduct.ImageURL);
                                    if (System.IO.File.Exists(imagePath))
                                    {
                                        System.IO.File.Delete(imagePath);
                                    }
                                }

                                // Guardar la nueva imagen en el servidor
                                string fileName = Path.GetFileName(imageFile.FileName);
                                string uploadDir = Path.Combine(Server.MapPath("~/Uploads"), fileName);
                                imageFile.SaveAs(uploadDir);

                                // Actualizar la ruta de la imagen en el modelo
                                existingProduct.ImageURL = "/Uploads/" + fileName;
                            }

                            // Actualizar otros campos del producto
                            existingProduct.Name = product.Name;
                            existingProduct.SupplierID = product.SupplierID;
                            existingProduct.CategoryID = product.CategoryID;
                            existingProduct.SubCategoryID = product.SubCategoryID;
                            existingProduct.UnitPrice = product.UnitPrice;
                            existingProduct.OldPrice = product.OldPrice;
                            existingProduct.UnitWeight = product.UnitWeight;
                            existingProduct.Discount = product.Discount;
                            existingProduct.UnitInStock = product.UnitInStock;
                            existingProduct.UnitOnOrder = product.UnitOnOrder;
                            existingProduct.ProductAvailable = product.ProductAvailable;
                            existingProduct.AltText = product.AltText;
                            existingProduct.AddBadge = product.AddBadge;
                            existingProduct.OfferTitle = product.OfferTitle;
                            existingProduct.OfferBadgeClass = product.OfferBadgeClass;
                            existingProduct.ShortDescription = product.ShortDescription;
                            existingProduct.LongDescription = product.LongDescription;

                            // Aplicar los cambios al contexto de Entity Framework
                            db.Entry(existingProduct).State = EntityState.Modified;
                            db.SaveChanges(); // Guardar cambios en la base de datos
                        }
                        else
                        {
                            ModelState.AddModelError("", "Producto no encontrado en la base de datos.");
                            return View(product);
                        }
                    }

                    return RedirectToAction("Index", "Productos"); // Redirigir a la página de listado de productos
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al intentar guardar los cambios: " + ex.Message);
                }
            }

            // Si llegamos aquí, hay un error de validación, devolver la vista con el modelo
            ViewBag.Suppliers = new SelectList(db.Suppliers, "SupplierID", "CompanyName", product.SupplierID);
            ViewBag.Categories = new SelectList(db.Categories, "CategoryID", "Name", product.CategoryID);
            ViewBag.SubCategories = new SelectList(db.SubCategories, "SubCategoryID", "Name", product.SubCategoryID);
            return View(product);
        }





        // GET: Productos/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Products product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Productos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Products product = db.Products.Find(id);
            db.Products.Remove(product);
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

        [HttpGet]
        public JsonResult GetSubCategories(string q)
        {
            var subCategories = db.SubCategories
                                 .Where(sc => sc.Name.StartsWith(q))
                                 .Select(sc => new { id = sc.SubCategoryID, text = sc.Name })
                                 .ToList();

            return Json(subCategories, JsonRequestBehavior.AllowGet);
        }



    }
}
