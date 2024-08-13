using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Khareedo.Models;


namespace Khareedo.Controllers
{
    public class OrdersController : Controller
    {
        private Entities db = new Entities(); // Contexto de la base de datos

        // GET: Orders
        public ActionResult Index()
        {
            // Obtener la lista de órdenes del usuario actualmente logeado
            int? customerId = Session["CustomerID"] as int?;
            if (customerId == null)
            {
                // Si no hay un CustomerID en la sesión, redirigir al usuario al login
                return RedirectToAction("Login", "Account");
            }

            var orders = db.Orders
                            .Where(o => o.CustomerID == customerId)
                            .ToList();

            return View(orders); // Pasa la lista de órdenes a la vista
        }
              public ActionResult Details(int id)
        {
            Order ord = db.Orders.Find(id);
            var Ord_details = db.OrderDetails.Where(x => x.OrderID == id).ToList();
            var tuple = new Tuple<Order, IEnumerable<OrderDetails>>(ord, Ord_details);

            double SumAmount = Convert.ToDouble(Ord_details.Sum(x => x.TotalAmount));
            ViewBag.TotalItems = Ord_details.Sum(x => x.Quantity);
            ViewBag.Discount = 0;
            ViewBag.TAmount = SumAmount - 0;
            ViewBag.Amount = SumAmount;
            return View(tuple);
        }
    }

}

