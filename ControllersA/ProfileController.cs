using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Khareedo.Models;
using IMS_Project.Models;
using System.Data;

namespace IMS_Project.Controllers
{
    public class ProfileController : Controller
    {
        Entities db = new Entities();
        // GET: Profile
        public ActionResult Index()
        {
            return View(db.AdminUsers.Find(TemData.EmpID));

        }

        public ActionResult Edit(int id)
        {
            AdminUsers emp =  db.AdminUsers.Find(id);
          if (emp==null)
          {
              return HttpNotFound();
          }
           return View(emp);
        }

        [HttpPost]
        public ActionResult Edit(AdminUsers emp)
        {
            if (ModelState.IsValid)
            {
                db.Entry(emp).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}