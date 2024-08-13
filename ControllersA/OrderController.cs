using System;
using System.Collections.Generic;
using System.Data.Objects.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Khareedo.Models;
using PagedList;

namespace IMS_Project.Controllers
{
    public class OrderController : Controller
    {
        Entities db = new Entities();
        // GET: Order
        public ActionResult Index(string searchString, int? page)
        {
            // Obtener la lista de órdenes ordenadas descendentemente por OrderID
            IQueryable<Order> ordenes = db.Orders.OrderByDescending(x => x.OrderID);

            // Aplicar búsqueda si hay un término de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                // Filtrar por OrderID como cadena
                ordenes = ordenes.Where(x => SqlFunctions.StringConvert((double)x.OrderID).Contains(searchString))
                                 .OrderByDescending(x => x.OrderID); // Asegurar ordenación después de filtrar
            }

            // Definir el número de elementos por página
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            // Devolver la vista Index con la lista de órdenes paginadas como modelo
            return View("~/Views/ViewsA/Order/Index.cshtml", ordenes.ToPagedList(pageNumber, pageSize));
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
            return View("~/Views/ViewsA/Order/Details.cshtml", tuple);
        }
    }
}