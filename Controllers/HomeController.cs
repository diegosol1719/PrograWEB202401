using Khareedo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Khareedo.Controllers
{
    public class HomeController : Controller
    {
        Entities db = new Entities();

        // GET: Home
        public ActionResult Index()
        {
            ViewBag.MenProduct = db.Products.Where(x => x.Categories.Name.Equals("Men")).ToList();
            ViewBag.WomenProduct = db.Products.Where(x => x.Categories.Name.Equals("Women")).ToList();
            ViewBag.SportsProduct = db.Products.Where(x => x.Categories.Name.Equals("Sports")).ToList();
            ViewBag.ElectronicsProduct = db.Products.Where(x => x.Categories.Name.Equals("Phones")).ToList();
            ViewBag.Slider = db.genMainSlider.ToList();
            ViewBag.PromoRight = db.genPromoRight.ToList();

            this.GetDefaultData();

            return View();
        }      

    }
}