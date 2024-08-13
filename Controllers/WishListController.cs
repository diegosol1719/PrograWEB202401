using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Khareedo.Models;

namespace Khareedo.Controllers
{
    public class WishListController : Controller
    {
        Entities db = new Entities();
        // GET: WishList
        public ActionResult Index()
        {
            this.GetDefaultData(); // Método para obtener datos por defecto, según tu implementación

            // Verifica que TempShpData.UserID tenga el valor correcto del usuario actual
            int userId = TempShpData.UserID; // Asumiendo que TempShpData.UserID es el ID del usuario actual

            // Obtener número de productos en la lista de deseos del usuario actual
            ViewBag.WlItemsNo = db.Wishlists.Count(x => x.CustomerID == userId);

            // Obtener productos de la lista de deseos del usuario actual
            var wishlistProducts = db.Wishlists.Where(x => x.CustomerID == userId).ToList();

            return View(wishlistProducts);
        }


        public ActionResult Indexx()
        {
            this.GetDefaultData(); // Método para obtener datos por defecto, según tu implementación

            // Obtener todas las órdenes completadas del usuario actual
            var completedOrders = db.Orders
                                   .Where(o => o.CustomerID == TempShpData.UserID && o.isCompleted == true)
                                   .ToList();

            if (completedOrders == null || completedOrders.Count == 0)
            {
                // Manejar caso donde no hay órdenes completadas para el usuario actual
                ViewBag.Message = "No se encontraron órdenes para mostrar.";
            }

            return View(completedOrders); // Pasar la lista de órdenes completadas a la vista
        }



        //REMOVE ITEM FROM WISHLIST
        public ActionResult Remove(int id)
        {
            db.Wishlists.Remove(db.Wishlists.Find(id));
            db.SaveChanges();
            return RedirectToAction("Index");

        }
        //ADD TO CART WISHLIST
        public ActionResult AddToCart(int id)
        {
            OrderDetails OD = new OrderDetails();

            int pid = db.Wishlists.Find(id).ProductID;
            OD.ProductID = pid;
            int Qty = 1;
            decimal price = db.Products.Find(pid).UnitPrice;
            OD.Quantity = Qty;
            OD.UnitPrice = price;
            OD.TotalAmount = Qty * price;
            OD.Products = db.Products.Find(pid);

            if (TempShpData.items == null)
            {
                TempShpData.items = new List<OrderDetails>();
            }
            TempShpData.items.Add(OD);

            db.Wishlists.Remove(db.Wishlists.Find(id));
            db.SaveChanges();

            return RedirectToAction("Index", "Wishlist");

        }
    }
}