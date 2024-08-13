using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using Khareedo.Models;
using PagedList;
using PagedList.Mvc;

namespace Khareedo.Controllers
{
    public class ProductController : Controller
    {
        Entities db = new Entities();


        public ActionResult Index()
        {
            var activeCategories = db.Categories.Where(c => c.isActive == true).ToList();
            ViewBag.ActiveCategories = activeCategories;
            ViewBag.Categories = db.Categories.Select(x => x.Name).ToList();

            ViewBag.TopRatedProducts = TopSoldProducts();
            ViewBag.RecentViewsProducts = RecentViewProducts();

            return View("Products");
        }

        //TOP SOLD PRODUCTS
        public List<TopSoldProduct> TopSoldProducts()
        {
            List<TopSoldProduct> topSoldProds = new List<TopSoldProduct>();

            try
            {
                var prodList = (from prod in db.OrderDetails
                                select new { prod.ProductID, prod.Quantity } into p
                                group p by p.ProductID into g
                                select new
                                {
                                    pID = g.Key,
                                    sold = g.Sum(x => x.Quantity)
                                }).OrderByDescending(y => y.sold).Take(3).ToList();

                foreach (var item in prodList)
                {
                    var product = db.Products.Find(item.pID);
                    if (product != null)
                    {
                        topSoldProds.Add(new TopSoldProduct()
                        {
                            product = product,
                            CountSold = Convert.ToInt32(item.sold)
                        });
                    }
                    else
                    {
                        // Puedes omitir este producto si no se encuentra en la base de datos
                        // O realizar algún registro o manejo específico aquí
                        Console.WriteLine($"Producto con ID {item.pID} no encontrado en la base de datos.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones: puedes agregar código aquí para registrar el error o manejarlo de otra manera adecuada
                Console.WriteLine("Error al recuperar los productos más vendidos: " + ex.Message);
                // Puedes elegir lanzar la excepción nuevamente si necesitas que sea manejada en niveles superiores
                // throw;
            }

            return topSoldProds;
        }

        //RECENT VIEWS PRODUCTS
        public IEnumerable<Products> RecentViewProducts()
        {
            try
            {
                if (TempShpData.UserID > 0)
                {
                    var top3Products = (from recent in db.RecentlyViews
                                        where recent.CustomerID == TempShpData.UserID
                                        orderby recent.ViewDate descending
                                        select recent.ProductID).Take(3).ToList();

                    var recentViewProd = db.Products.Where(x => top3Products.Contains(x.ProductID)).ToList();
                    return recentViewProd;
                }
                else
                {
                    var prod = db.Products.OrderByDescending(x => x.UnitPrice).Take(3).ToList();
                    return prod;
                }
            }
            catch (Exception ex)
            {
                // Manejo de excepciones: puedes agregar código aquí para registrar el error o manejarlo de otra manera adecuada
                Console.WriteLine("Error al recuperar productos recientes o productos por precio: " + ex.Message);
                // Puedes elegir lanzar la excepción nuevamente si necesitas que sea manejada en niveles superiores
                // throw;

                // En caso de error, puedes devolver una colección vacía o null, dependiendo de tu manejo de errores
                return Enumerable.Empty<Products>();
            }
        }


        //ADD TO CART
        public ActionResult AddToCart(int id)
        {
            OrderDetails OD = new OrderDetails();
            OD.ProductID = id;
            int Qty = 1;
            decimal price = db.Products.Find(id).UnitPrice;
            OD.Quantity = Qty;
            OD.UnitPrice = price;
            OD.TotalAmount = Qty * price;
            OD.Products = db.Products.Find(id);

            if (TempShpData.items == null)
            {
                TempShpData.items = new List<OrderDetails>();
            }
            TempShpData.items.Add(OD);
            AddRecentViewProduct(id);
            return Redirect(TempData["returnURL"].ToString());

        }

        //VIEW DETAILS
        public ActionResult ViewDetails(int id)
        {
            var prod = db.Products.Find(id);
            var reviews = db.Reviews.Where(x => x.ProductID == id).ToList();
            ViewBag.Reviews = reviews;
            ViewBag.TotalReviews = reviews.Count();
            ViewBag.RelatedProducts = db.Products.Where(y => y.CategoryID == prod.CategoryID).ToList();
            AddRecentViewProduct(id);

            var ratedProd=db.Reviews.Where(x => x.ProductID == id).ToList();
            int count = ratedProd.Count();
            int TotalRate =  ratedProd.Sum(x => x.Rate).GetValueOrDefault();
            ViewBag.AvgRate = TotalRate > 0 ? TotalRate / count : 0;

            this.GetDefaultData();
            return View(prod);
        }

        //WISHLIST
        public ActionResult WishList(int id)
        {
            try
            {
                Wishlist wl = new Wishlist();
                wl.ProductID = id;
                wl.CustomerID = TempShpData.UserID;

                db.Wishlists.Add(wl);
                db.SaveChanges();

                AddRecentViewProduct(id);

                ViewBag.WlItemsNo = db.Wishlists.Where(x => x.CustomerID == TempShpData.UserID).ToList().Count();

                if (TempData["returnURL"].ToString() == "/")
                {
                    return RedirectToAction("Index", "Home");
                }

                return Redirect(TempData["returnURL"].ToString());
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is SqlException sqlEx && sqlEx.Number == 547)
                {
                    ViewBag.ErrorMessage = "No se pudo agregar el producto a la lista de deseos. El cliente no existe.";
                }
                else
                {
                    ViewBag.ErrorMessage = "Ocurrió un error al procesar la solicitud. Por favor, inténtalo nuevamente más tarde.";
                }

                // Aquí podrías agregar más lógica de manejo de errores si es necesario

                return View(); // O redirigir a una vista de error, según tu implementación
            }
            catch (Exception)
            {
                ViewBag.ErrorMessage = "Ocurrió un error inesperado. Por favor, inténtalo nuevamente más tarde.";

                // Aquí podrías agregar más lógica de manejo de errores si es necesario

                return View(); // O redirigir a una vista de error, según tu implementación
            }
        }


        //ADD RECENT VIEWS PRODUCT IN DB
        public void AddRecentViewProduct(int pid)
        {
            if (TempShpData.UserID > 0)
            {
                RecentlyViews Rv = new RecentlyViews();
                Rv.CustomerID = TempShpData.UserID;
                Rv.ProductID = pid;
                Rv.ViewDate = DateTime.Now;
                db.RecentlyViews.Add(Rv);
                db.SaveChanges();
            }
        }

        //ADD REVIEWS ABOUT PRODUCT
        public ActionResult AddReview(int productID, FormCollection getReview)
        {
            int customerID = TempShpData.UserID;
            Customers customer = db.Customers.Find(customerID);

            Review r = new Review();
            r.CustomerID = TempShpData.UserID;
            r.ProductID = productID;
            r.Name = customer.UserName;
            r.Email = getReview["email"];
            r.Review1 = getReview["message"];
            r.Rate = Convert.ToInt32(getReview["rate"]);
            r.DateTime = DateTime.Now;

            db.Reviews.Add(r);
            db.SaveChanges();
            return RedirectToAction("ViewDetails/" + productID + "");

        }


        public ActionResult Products(int subCatID)
        {
            ViewBag.Categories = db.Categories.Select(x => x.Name).ToList();
            var prods = db.Products.Where(y => y.SubCategoryID == subCatID).ToList();
            return View(prods);
        }

        //GET PRODUCTS BY CATEGORY
        // GET PRODUCTS BY CATEGORY
        public ActionResult GetProductsByCategory(string categoryName, int? page)
        {
            ViewBag.Categories = db.Categories.Select(x => x.Name).ToList();
            ViewBag.TopRatedProducts = TopSoldProducts();
            ViewBag.RecentViewsProducts = RecentViewProducts();
            ViewBag.NoOfItem = TempShpData.items?.Count ?? 0;
            ViewBag.Total = TempShpData.items?.Sum(x => x.TotalAmount) ?? 0;
            ViewBag.cartBox = TempShpData.items;

            var prods = db.Products.Where(x => x.Categories.Name == categoryName).ToList();
            return View("Products", prods.ToPagedList(page ?? 1, 9));
        }


        //SEARCH BAR
        public ActionResult Search(string product,int? page)
        {
            ViewBag.Categories = db.Categories.Select(x => x.Name).ToList();
            ViewBag.TopRatedProducts = TopSoldProducts();
          
            ViewBag.RecentViewsProducts = RecentViewProducts();

            List<Products> products;
            if (!string.IsNullOrEmpty(product))
            {
                products = db.Products.Where(x => x.Name.StartsWith(product)).ToList();
            }
            else
            {
                products = db.Products.ToList();
            }
            return View("Products", products.ToPagedList(page ?? 1,6));
        }

        public JsonResult GetProducts(string term)
        {
            List<string> prodNames = db.Products.Where(x => x.Name.StartsWith(term)).Select(y => y.Name).ToList();
            return Json(prodNames, JsonRequestBehavior.AllowGet);

        }
        public ActionResult FilterByPrice(int minPrice,int maxPrice,int? page)
        {
            ViewBag.Categories = db.Categories.Select(x => x.Name).ToList();
            ViewBag.TopRatedProducts = TopSoldProducts();

            ViewBag.RecentViewsProducts = RecentViewProducts();
            ViewBag.filterByPrice = true;
           var filterProducts= db.Products.Where(x => x.UnitPrice >= minPrice && x.UnitPrice <= maxPrice).ToList();
           return View("Products", filterProducts.ToPagedList(page ?? 1, 9));
        }

       
    }
}