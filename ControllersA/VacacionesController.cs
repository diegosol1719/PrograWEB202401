using System;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity; 
using Khareedo.Models;
using System.Data;
using System.Collections.Generic;
using System.IO;
using PagedList;

namespace Khareedo.Controllers
{
    public class VacacionesController : Controller
    {
        private readonly Entities _context = new Entities();




        public void IncrementarDiasVacaciones()
        {
            try
            {
                var fechaUltimaEjecucion = ObtenerFechaUltimaEjecucion();
                var ahora = DateTime.Now;

                if (fechaUltimaEjecucion == null ||
                    fechaUltimaEjecucion.Value.Year != ahora.Year ||
                    fechaUltimaEjecucion.Value.Month != ahora.Month)
                {
                    // Calcula la cantidad de meses transcurridos
                    int mesesTranscurridos = ((ahora.Year - fechaUltimaEjecucion.Value.Year) * 12) + ahora.Month - fechaUltimaEjecucion.Value.Month;

                    var users = _context.AdminUsers.ToList();
                    var usuariosAfectados = new List<string>();

                    foreach (var user in users)
                    {
                        if (user.DiasVacaciones == null)
                        {
                            user.DiasVacaciones = mesesTranscurridos;
                        }
                        else
                        {
                            user.DiasVacaciones += mesesTranscurridos;
                        }

                        usuariosAfectados.Add($"UsuarioID: {user.UserID}, DiasVacaciones: {user.DiasVacaciones}");
                    }

                    _context.SaveChanges();
                    CrearArchivoUsuariosAfectados(usuariosAfectados);

                    // Guardar la fecha de la última ejecución
                    GuardarFechaUltimaEjecucion(ahora);
                }
            }
            catch (Exception ex)
            {
                CrearArchivoUsuariosAfectados(new List<string> { $"Error: {ex.Message}" });
            }
        }

        private void CrearArchivoUsuariosAfectados(List<string> usuariosAfectados)
        {
            try
            {
                var archivoUsuariosAfectadosPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Vacacionestxt", "UsuariosAfectados.txt");
                Directory.CreateDirectory(Path.GetDirectoryName(archivoUsuariosAfectadosPath));
                System.IO.File.WriteAllLines(archivoUsuariosAfectadosPath, usuariosAfectados);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al crear el archivo de usuarios afectados: {ex.Message}");
            }
        }

        private DateTime? ObtenerFechaUltimaEjecucion()
        {
            try
            {
                var archivoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Vacacionestxt", "fecha_ultima_ejecucion.txt");
                if (System.IO.File.Exists(archivoPath))
                {
                    var fechaTexto = System.IO.File.ReadAllText(archivoPath);
                    if (DateTime.TryParse(fechaTexto, out DateTime fechaUltima))
                    {
                        return fechaUltima;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener la fecha de la última ejecución: {ex.Message}");
            }
            return null;
        }

        private void GuardarFechaUltimaEjecucion(DateTime fecha)
        {
            try
            {
                var archivoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Vacacionestxt", "fecha_ultima_ejecucion.txt");
                System.IO.File.WriteAllText(archivoPath, fecha.ToString("yyyy-MM-dd"));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar la fecha de la última ejecución: {ex.Message}");
            }
        }









        [HttpGet]
        public JsonResult GetDiasDisponibles()
        {
            var userID = Session["EmpID"] as int?;
            if (userID == null)
            {
                return Json(new { diasDisponibles = 0 }, JsonRequestBehavior.AllowGet);
            }

            var user = _context.AdminUsers.Find(userID);
            var diasDisponibles = user?.DiasVacaciones ?? 0;
            return Json(new { diasDisponibles }, JsonRequestBehavior.AllowGet);
        }



        private int GetUserRole()
        {
            if (Session["RoleType"] != null)
            {
                return (int)Session["RoleType"];
            }
            return 0; // Valor predeterminado si no se encuentra el rol
        }

        // GET: Vacaciones
        public ActionResult Index(string searchString, int? page)
        {
            var userRole = GetUserRole();
            TempData["UserRole"] = userRole;

            int pageSize = 10; // Número de elementos por página
            int pageNumber = page ?? 1; // Número de página actual

            IQueryable<Vacaciones> vacacionesQuery = _context.Vacaciones.Include("AdminUsers");

            if (userRole == 0) // Si el rol es 0, filtra solo las vacaciones del usuario logueado
            {
                var userId = (int)Session["EmpID"];
                vacacionesQuery = vacacionesQuery.Where(v => v.UserID == userId);
            }

            // Filtra por nombre si se proporciona una cadena de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                vacacionesQuery = vacacionesQuery
                    .Where(v => (v.AdminUsers.FirstName + " " + v.AdminUsers.LastName).Contains(searchString));
            }

            // Aplica paginación y ordena por ID de vacaciones
            var vacaciones = vacacionesQuery
                .OrderBy(v => v.VacacionesID)
                .ToPagedList(pageNumber, pageSize);

            ViewBag.SearchString = searchString;

            return View("~/Views/ViewsA/Vacaciones/Index.cshtml", vacaciones);
        }





        public ActionResult Create()
        {
            var userID = Session["EmpID"] as int?;
            var userRole = GetUserRole(); // Obtén el rol del usuario

            if (userID == null)
            {
                ModelState.AddModelError("", "No se pudo obtener el ID del usuario.");

                // Obtener la lista completa de usuarios y filtrarlos después de la recuperación
                var users = _context.AdminUsers
                    .ToList() // Primero obtén los datos
                    .Select(u => new SelectListItem
                    {
                        Value = u.UserID.ToString(),
                        Text = $"ID-{u.UserID} {u.FirstName} {u.LastName}"
                    })
                    .ToList();

                // Si el rol es 0, muestra solo el usuario logueado en el dropdown
                if (userRole == 0)
                {
                    users = users.Where(u => u.Value == userID.ToString()).ToList();
                }

                // Convertir a SelectList
                ViewBag.Users = new SelectList(users, "Value", "Text");

                return View("~/Views/ViewsA/Vacaciones/Create.cshtml");
            }

            // Obtén los días disponibles del usuario
            var diasDisponibles = _context.AdminUsers
                .Where(u => u.UserID == userID)
                .Select(u => u.DiasVacaciones)
                .FirstOrDefault();

            var model = new Vacaciones
            {
                UserID = userID.Value,
                FechaInicio = DateTime.Now,
                FechaFin = DateTime.Now,
                Motivo = string.Empty
            };

            // Obtener la lista de usuarios para el DropDownList
            var allUsers = _context.AdminUsers
                .ToList() // Primero obtén los datos
                .Select(u => new SelectListItem
                {
                    Value = u.UserID.ToString(),
                    Text = $"ID-{u.UserID} {u.FirstName} {u.LastName}"
                })
                .ToList();

            if (userRole == 0)
            {
                // Filtrar solo el usuario logueado si el rol es 0
                allUsers = allUsers.Where(u => u.Value == userID.ToString()).ToList();
            }

            // Convertir a SelectList
            ViewBag.Users = new SelectList(allUsers, "Value", "Text");

            // Pasar los días disponibles a la vista
            ViewBag.DiasDisponibles = diasDisponibles;

            return View("~/Views/ViewsA/Vacaciones/Create.cshtml", model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Vacaciones vacaciones)
        {
            if (ModelState.IsValid)
            {
                var diasSeleccionados = (vacaciones.FechaFin - vacaciones.FechaInicio).Days + 1;
                var usuario = _context.AdminUsers.Find(vacaciones.UserID);

                if (usuario == null)
                {
                    ModelState.AddModelError("", "Usuario no encontrado.");

                    // Obtener la lista completa de usuarios para el dropdown
                    var allUsersList = _context.AdminUsers
                        .ToList()
                        .Select(u => new SelectListItem
                        {
                            Value = u.UserID.ToString(),
                            Text = $"ID-{u.UserID} {u.FirstName} {u.LastName}"
                        })
                        .ToList();
                    ViewBag.Users = new SelectList(allUsersList, "Value", "Text");

                    return View("~/Views/ViewsA/Vacaciones/Create.cshtml", vacaciones);
                }

                // Verificar si el usuario tiene suficientes días de vacaciones
                if (diasSeleccionados > usuario.DiasVacaciones)
                {
                    ModelState.AddModelError("", "No tienes suficientes días de vacaciones disponibles.");

                    // Obtener la lista completa de usuarios para el dropdown
                    var allUsersList = _context.AdminUsers
                        .ToList()
                        .Select(u => new SelectListItem
                        {
                            Value = u.UserID.ToString(),
                            Text = $"ID-{u.UserID} {u.FirstName} {u.LastName}"
                        })
                        .ToList();
                    ViewBag.Users = new SelectList(allUsersList, "Value", "Text");

                    return View("~/Views/ViewsA/Vacaciones/Create.cshtml", vacaciones);
                }

                // Restar los días seleccionados de los días de vacaciones del usuario
                usuario.DiasVacaciones -= diasSeleccionados;
                _context.Entry(usuario).State = EntityState.Modified;

                vacaciones.DiasVacaciones = diasSeleccionados;
                if (string.IsNullOrWhiteSpace(vacaciones.Estado))
                {
                    vacaciones.Estado = "Pendiente";
                }
                vacaciones.FechaSolicitud = DateTime.Now;

                _context.Vacaciones.Add(vacaciones);
                _context.SaveChanges();

                return RedirectToAction("Index");
            }

            // Repetir la lógica de obtener usuarios en caso de error
            var allUsersForView = _context.AdminUsers
                .ToList()
                .Select(u => new SelectListItem
                {
                    Value = u.UserID.ToString(),
                    Text = $"ID-{u.UserID} {u.FirstName} {u.LastName}"
                })
                .ToList();
            ViewBag.Users = new SelectList(allUsersForView, "Value", "Text");

            return View("~/Views/ViewsA/Vacaciones/Create.cshtml", vacaciones);
        }

        // GET: Vacaciones/Edit/5
        public ActionResult Edit(int id)
        {
            var vacaciones = _context.Vacaciones.Find(id);
            if (vacaciones == null)
            {
                return HttpNotFound();
            }
            return View(vacaciones);
        }

        // POST: Vacaciones/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Vacaciones vacaciones)
        {
            if (ModelState.IsValid)
            {
                // Busca la entidad existente en el contexto
                var existingVacaciones = _context.Vacaciones.Find(vacaciones.VacacionesID);

                if (existingVacaciones != null)
                {
                    // Actualiza las propiedades de la entidad existente
                    existingVacaciones.FechaInicio = vacaciones.FechaInicio;
                    existingVacaciones.FechaFin = vacaciones.FechaFin;
                    existingVacaciones.Motivo = vacaciones.Motivo;
                    existingVacaciones.DiasVacaciones = (vacaciones.FechaFin - vacaciones.FechaInicio).Days + 1;

                    // Marca la entidad como modificada de forma manual
                    _context.Entry(existingVacaciones).State = EntityState.Modified;
                    _context.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(vacaciones);
        }



        // GET: Vacaciones/Delete/5
        public ActionResult Delete(int id)
        {
            var vacaciones = _context.Vacaciones.Find(id);
            if (vacaciones == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Vacaciones/Delete.cshtml",vacaciones);
        }

        // POST: Vacaciones/Delete/5
        // POST: Vacaciones/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            // Encuentra la solicitud de vacaciones que se está eliminando
            var vacaciones = _context.Vacaciones.Find(id);
            if (vacaciones == null)
            {
                return HttpNotFound();
            }

            // Obtén el número de días de la solicitud
            var diasEliminar = (vacaciones.FechaFin - vacaciones.FechaInicio).Days + 1;

            // Encuentra el usuario asociado con la solicitud
            var usuario = _context.AdminUsers.Find(vacaciones.UserID);
            if (usuario != null)
            {
                // Devuelve los días de vacaciones al usuario
                usuario.DiasVacaciones += diasEliminar;

                // Marca el usuario como modificado y guarda los cambios
                _context.Entry(usuario).State = EntityState.Modified;
            }

            // Elimina la solicitud de vacaciones
            _context.Vacaciones.Remove(vacaciones);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }



        // GET: Vacaciones/Aprobar/5
        public ActionResult Aprobar(int id)
        {
            var vacaciones = _context.Vacaciones
                .Include(v => v.AdminUsers) // Asegúrate de incluir AdminUsers
                .FirstOrDefault(v => v.VacacionesID == id);

            if (vacaciones == null)
            {
                return HttpNotFound();
            }
            return View("~/Views/ViewsA/Vacaciones/Aprobar.cshtml", vacaciones);
        }

        // POST: Vacaciones/Aprobar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Aprobar(int id, string estado)
        {
            var vacaciones = _context.Vacaciones.Find(id);
            if (vacaciones == null)
            {
                return HttpNotFound();
            }

            if (estado == "Aprobada")
            {
                vacaciones.Estado = "Aprobada";
                vacaciones.FechaAprobacion = DateTime.Now; 
            }
            else if (estado == "Rechazada")
            {
                vacaciones.Estado = "Rechazada";
                vacaciones.FechaAprobacion = DateTime.Now;
            }

            _context.Entry(vacaciones).State = EntityState.Modified;
            _context.SaveChanges();

            return RedirectToAction("Index");
        }



    }



}
