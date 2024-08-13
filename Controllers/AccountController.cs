using System.Linq;
using System.Web.Mvc;
using Khareedo.Models;
using System.Data.Entity;
using System.Data;
using System.IO;
using System.Web;
using System;

namespace Khareedo.Controllers
{
    public class AccountController : Controller
    {
        Entities db = new Entities();


        // GET: Account
        public ActionResult Index()
        {
            this.GetDefaultData();
            var customer = db.Customers.Find(TempShpData.UserID);
            return View(customer);
        }

        // LOG IN
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(string UserName, string Password)
        {
            if (ModelState.IsValid)
            {
                // Verifica si el usuario es un administrador
                var adminUser = db.AdminUsers.FirstOrDefault(m => m.UserName == UserName && m.Password == Password);

                if (adminUser != null)
                {
                    // Es un administrador
                    Session["username"] = adminUser.UserName;
                    Session["EmpID"] = adminUser.UserID;
                    Session["RoleType"] = adminUser.RoleType;
                    return RedirectToAction("Index", "Dashboard"); // Redirige a la página de administrador
                }
                else
                {
                    // Si no es un administrador, verifica si es un cliente
                    var customer = db.Customers.FirstOrDefault(m => m.UserName == UserName && m.Password == Password);
                    if (customer != null)
                    {
                        // Es un cliente
                        Session["CustomerID"] = customer.CustomerID;
                        TempShpData.UserID = customer.CustomerID;
                        Session["username"] = customer.UserName;
                        return RedirectToAction("Index", "Home"); // Se queda en la página principal del cliente
                    }
                    else
                    {
                        // Usuario o contraseña incorrectos
                        ModelState.AddModelError("", "Nombre de usuario o contraseña incorrectos.");
                    }
                }
            }
            return View();
        }



        // LOG OUT
        public ActionResult Logout()
        {
            Session["username"] = null;
            TempShpData.UserID = 0;
            TempShpData.items = null;
            return RedirectToAction("Index", "Home");
        }

        public Customers GetUser(string _usrName)
        {
            var cust = (from c in db.Customers
                        where c.UserName == _usrName
                        select c).FirstOrDefault();
            return cust;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(Customers model, HttpPostedFileBase imageFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Si se proporciona un archivo de imagen, procesarlo
                    if (imageFile != null && imageFile.ContentLength > 0)
                    {
                        string fileName = Path.GetFileName(imageFile.FileName);

                        // Ruta completa fuera del directorio raíz del proyecto
                        string uploadDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"ImagenesCompartidas");

                        // Verificar si la carpeta ImagenesCompartidas existe o crearla si no existe
                        if (!Directory.Exists(uploadDir))
                        {
                            Directory.CreateDirectory(uploadDir);
                        }

                        // Guardar la imagen en el servidor
                        string imagePath = Path.Combine(uploadDir, fileName);
                        imageFile.SaveAs(imagePath);

                        // Actualizar el modelo con la ruta de la imagen guardada
                        model.Picture = "/ImagenesCompartidas/" + fileName;
                    }

                    // Guardar el modelo actualizado en la base de datos
                    using (var db = new Entities())
                    {
                        var existingCustomer = db.Customers.Find(model.CustomerID);
                        if (existingCustomer != null)
                        {
                            // Actualizar todos los campos del cliente (incluyendo la imagen si se ha actualizado)
                            existingCustomer.First_Name = model.First_Name;
                            existingCustomer.Last_Name = model.Last_Name;
                            existingCustomer.Age = model.Age;
                            existingCustomer.Gender = model.Gender;
                            existingCustomer.DateofBirth = model.DateofBirth;
                            existingCustomer.Organization = model.Organization;
                            existingCustomer.Picture = model.Picture;
                            existingCustomer.Notes = model.Notes;
                            existingCustomer.Country = model.Country;
                            existingCustomer.State = model.State;
                            existingCustomer.City = model.City;
                            existingCustomer.PostalCode = model.PostalCode;
                            existingCustomer.Email = model.Email;
                            existingCustomer.AltEmail = model.AltEmail;
                            existingCustomer.Phone1 = model.Phone1;
                            existingCustomer.Phone2 = model.Phone2;
                            existingCustomer.Mobile1 = model.Mobile1;
                            existingCustomer.Mobile2 = model.Mobile2;
                            existingCustomer.Address1 = model.Address1;
                            existingCustomer.Address2 = model.Address2;
                            existingCustomer.UserName = model.UserName;
                            existingCustomer.Password = model.Password;

                            db.SaveChanges(); // Guardar cambios en la base de datos
                        }
                        else
                        {
                            ModelState.AddModelError("", "Cliente no encontrado en la base de datos.");
                            return View(model);
                        }
                    }

                    // Redirigir a la página principal después de guardar exitosamente
                    return RedirectToAction("Index", "Home");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al intentar guardar los cambios: " + ex.Message);
                }
            }

            // Si llega aquí, significa que ha ocurrido un error o la validación ha fallado
            return View(model);
        }


        private bool IsImageUrl(string url)
        {
            Uri uriResult;
            return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                   && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }



        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(Customers cust)
        {
            if (ModelState.IsValid)
            {
                var existingCustomer = db.Customers.FirstOrDefault(c => c.Email == cust.Email);
                if (existingCustomer != null)
                {
                    ViewData["ErrorMessage"] = "El correo electrónico ya está registrado.";
                    return View("Login", cust); // Redirige a la vista de Login con el modelo Customer
                }

                db.Customers.Add(cust);
                db.SaveChanges();

                Session["username"] = cust.UserName;
                TempShpData.UserID = GetUser(cust.UserName).CustomerID;
                return RedirectToAction("Index", "Home");
            }

            ViewData["ErrorMessage"] = "Los datos no son válidos.";
            return View("Login", cust); // Redirige a la vista de Login con el modelo Customer
        }



    }
}
