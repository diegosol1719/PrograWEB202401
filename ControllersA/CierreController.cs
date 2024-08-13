using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI;
using iTextSharp.text;
using Khareedo.Models; // Asegúrate de que este namespace contenga AdminUser
using PagedList;

namespace Khareedo.Controllers
{
    public class CierreController : Controller
    {
        private Entities db = new Entities();

        private int GetUserRole()
        {
            if (Session["RoleType"] != null)
            {
                return (int)Session["RoleType"];
            }
            return 0; 
        }

        public ActionResult Index(string searchString, int? page)
        {
            decimal cajaPercentage = 0.115m; // 11.5%
            decimal faltaPercentage = 0.04545m; // 4.545%
            decimal deduccionSalidaAnticipadaPercentage = 0.02m; // 2% 
            decimal deduccionLlegadaTardePercentage = 0.018m; // 1.8% 

            int userRole = GetUserRole();
            int pageSize = 10; // Número de elementos por página
            int pageNumber = page ?? 1; // Número de página actual

            IQueryable<AdminUsers> usuariosQuery = db.AdminUsers;

            if (userRole == 0)
            {
                // Verificar si la sesión no es nula y si se puede convertir a int
                if (Session["EmpID"] != null && int.TryParse(Session["EmpID"].ToString(), out int userId))
                {
                    usuariosQuery = usuariosQuery.Where(u => u.UserID == userId);
                }
                // Si la sesión es nula o la conversión falla, no hacer nada
                else
                {
                    // No hacer nada, simplemente continuar
                }
            }


            // Filtra por nombre si se proporciona una cadena de búsqueda
            if (!string.IsNullOrEmpty(searchString))
            {
                usuariosQuery = usuariosQuery
                    .Where(u => (u.FirstName + " " + u.LastName).Contains(searchString));
            }

            // Ordena los resultados antes de aplicar la paginación
            usuariosQuery = usuariosQuery.OrderBy(u => u.LastName); // Ordena por apellido, puedes cambiar el criterio

            var usuarios = usuariosQuery
                .Select(u => new
                {
                    u.UserID,
                    u.FirstName,
                    u.LastName,
                    u.Salario,
                    Asistencias = u.Asistencia
                })
                .ToList();

            var resultados = usuarios.Select(u => new CierrePlanillaViewModel
            {
                UserID = u.UserID,
                FirstName = u.FirstName,
                LastName = u.LastName,
                BaseSalary = u.Salario ?? 0,
                SalarioDiario = (u.Salario ?? 0) / 22,
                DeduccionSalidaAnticipada = (u.Salario ?? 0) * deduccionSalidaAnticipadaPercentage * u.Asistencias.Count(a => a.Estado == "Salida anticipada"),
                DeduccionFalta = (u.Salario ?? 0) * faltaPercentage * u.Asistencias.Count(a => a.Estado == "Falta"),
                DeduccionLlegadaTarde = (u.Salario ?? 0) * deduccionLlegadaTardePercentage * u.Asistencias.Count(a => a.Estado == "Llegó tarde"),
                CajaDeduction = (u.Salario ?? 0) * cajaPercentage,
                FinalSalary = (u.Salario ?? 0) -
                    ((u.Salario ?? 0) * deduccionSalidaAnticipadaPercentage * u.Asistencias.Count(a => a.Estado == "Salida anticipada") +
                     (u.Salario ?? 0) * faltaPercentage * u.Asistencias.Count(a => a.Estado == "Falta") +
                     (u.Salario ?? 0) * deduccionLlegadaTardePercentage * u.Asistencias.Count(a => a.Estado == "Llegó tarde") +
                     (u.Salario ?? 0) * cajaPercentage)
            }).ToList();

            // Aplicar paginación
            var pagedResult = resultados.ToPagedList(pageNumber, pageSize);

            ViewBag.SearchString = searchString;

            return View("~/Views/ViewsA/Cierreplanilla/Index.cshtml", pagedResult);
        }




        public ActionResult Details(int id)
        {
            decimal cajaPercentage = 0.115m; // 11.5%
            decimal faltaPercentage = 0.04545m; // 4.545%
            decimal deduccionSalidaAnticipadaPercentage = 0.02m; // 2% 
            decimal deduccionLlegadaTardePercentage = 0.018m; // 1.8% 

            var usuario = db.AdminUsers
                .Where(u => u.UserID == id)
                .Select(u => new CierrePlanillaViewModel
                {
                    UserID = u.UserID,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    BaseSalary = u.Salario ?? 0,
                    SalarioDiario = (u.Salario ?? 0) / 22,
                    DeduccionSalidaAnticipada = (u.Salario ?? 0) * deduccionSalidaAnticipadaPercentage * u.Asistencia.Count(a => a.Estado == "Salida anticipada"),
                    DeduccionFalta = (u.Salario ?? 0) * faltaPercentage * u.Asistencia.Count(a => a.Estado == "Falta"),
                    DeduccionLlegadaTarde = (u.Salario ?? 0) * deduccionLlegadaTardePercentage * u.Asistencia.Count(a => a.Estado == "Llegó tarde"),
                    CajaDeduction = (u.Salario ?? 0) * cajaPercentage,
                    FinalSalary = (u.Salario ?? 0) -
                        ((u.Salario ?? 0) * deduccionSalidaAnticipadaPercentage * u.Asistencia.Count(a => a.Estado == "Salida anticipada") +
                         (u.Salario ?? 0) * faltaPercentage * u.Asistencia.Count(a => a.Estado == "Falta") +
                         (u.Salario ?? 0) * deduccionLlegadaTardePercentage * u.Asistencia.Count(a => a.Estado == "Llegó tarde") +
                         (u.Salario ?? 0) * cajaPercentage)
                })
                .FirstOrDefault();

            if (usuario == null)
            {
                return HttpNotFound();
            }

            return View("~/Views/ViewsA/Cierreplanilla/Details.cshtml", usuario); // Ruta completa a la vista
        }


        // Clase interna para el modelo de vista
        public class CierrePlanillaViewModel
        {
            public int UserID { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public decimal BaseSalary { get; set; }
            public decimal SalarioDiario { get; set; }
            public decimal DeduccionSalidaAnticipada { get; set; }
            public decimal DeduccionFalta { get; set; }
            public decimal DeduccionLlegadaTarde { get; set; }
            public decimal CajaDeduction { get; set; }
            public decimal FinalSalary { get; set; }
        }
    }
}
