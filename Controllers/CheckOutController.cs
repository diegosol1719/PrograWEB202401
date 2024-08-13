using System;
using System.Linq;
using System.Web.Mvc;
using Khareedo.Models;
using System.Data.Entity;
using System.Data;

namespace Khareedo.Controllers
{
    public class CheckOutController : Controller
    {
        Entities db = new Entities();

        // GET: CheckOut
        // GET: CheckOut
        public ActionResult Index()
        {
            // Obtener los datos del cliente desde la base de datos
            int customerId = TempShpData.UserID; // Suponiendo que obtienes el ID del cliente de alguna manera
            Customers customer = db.Customers.Find(customerId); // Buscar el cliente por su ID

            // Verificar si se encontró el cliente
            if (customer == null)
            {
                // Manejar el caso donde no se encuentra el cliente, por ejemplo, redirigir a una página de error o a otro lugar
                return RedirectToAction("Index", "Home"); // Por ejemplo, redirigir a la página principal
            }

            // Pasar los datos del cliente a la vista
            ViewBag.Customer = customer;

            // Obtener otros datos necesarios para la vista
            ViewBag.PayMethod = new SelectList(db.PaymentTypes, "PayTypeID", "TypeName");
            var data = this.GetDefaultData(); // Supongamos que esto obtiene otros datos necesarios para la vista
            return View(data);
        }


        // PLACE ORDER -- LAST STEP
        [HttpPost]
        public ActionResult PlaceOrder(FormCollection getCheckoutDetails)
        {
            try
            {
                int shpID = 1;
                if (db.ShippingDetails.Any())
                {
                    shpID = db.ShippingDetails.Max(x => x.ShippingID) + 1;
                }
                int payID = 1;
                if (db.Payments.Any())
                {
                    payID = db.Payments.Max(x => x.PaymentID) + 1;
                }
                int orderID = 1;
                if (db.Orders.Any())
                {
                    orderID = db.Orders.Max(x => x.OrderID) + 1;
                }

                ShippingDetails shpDetails = new ShippingDetails();
                shpDetails.ShippingID = shpID;
                shpDetails.FirstName = getCheckoutDetails["FirstName"];
                shpDetails.LastName = getCheckoutDetails["LastName"];
                shpDetails.Email = getCheckoutDetails["Email"];
                shpDetails.Mobile = getCheckoutDetails["Mobile"];
                shpDetails.Address = getCheckoutDetails["Address"];
                shpDetails.Province = getCheckoutDetails["Province"];
                shpDetails.City = getCheckoutDetails["City"];
                shpDetails.PostCode = getCheckoutDetails["PostCode"];
                db.ShippingDetails.Add(shpDetails);
                db.SaveChanges();

                Payment pay = new Payment();
                pay.PaymentID = payID;
                pay.Type = Convert.ToInt32(getCheckoutDetails["PayMethod"]);
                db.Payments.Add(pay);
                db.SaveChanges();

                Order o = new Order();
                o.OrderID = orderID;
                o.CustomerID = TempShpData.UserID;
                o.PaymentID = payID;
                o.ShippingID = shpID;
                o.Discount = Convert.ToInt32(getCheckoutDetails["discount"]);
                o.TotalAmount = Convert.ToInt32(getCheckoutDetails["totalAmount"]);
                o.isCompleted = true;
                o.OrderDate = DateTime.Now;
                db.Orders.Add(o);
                db.SaveChanges();

                foreach (var OD in TempShpData.items)
                {
                    OD.OrderID = orderID;
                    OD.Order = db.Orders.Find(orderID);
                    OD.Products = db.Products.Find(OD.ProductID);

                    // Reducir la cantidad del producto en la base de datos
                    if (OD.Products.UnitInStock >= OD.Quantity)
                    {
                        OD.Products.UnitInStock -= OD.Quantity;
                        db.Entry(OD.Products).State = EntityState.Modified;
                        db.SaveChanges(); // Guardar cambios de inmediato
                    }
                    else
                    {
                        // Manejar el caso donde no hay suficiente cantidad
                        TempData["ErrorMessage"] = "No hay suficiente cantidad de " + OD.Products.Name;
                        return RedirectToAction("Index", "CheckOut");
                    }

                    db.OrderDetails.Add(OD);
                    db.SaveChanges();
                }

                // Limpiar datos temporales después de completar el pedido
                TempShpData.items.Clear();
                TempShpData.UserID = -1;
                return RedirectToAction("Index", "ThankYou");
            }
            catch (Exception ex)
            {
                // Manejar la excepción
                return RedirectToAction("Index", "CheckOut", new { message = ex.Message });
            }
        }

    }
}
