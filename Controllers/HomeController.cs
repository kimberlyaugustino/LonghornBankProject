using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using LonghornBankProject.Models;
using Microsoft.AspNet.Identity;
using System.Web.Mvc;

namespace LonghornBankProject.Controllers
{
    public class HomeController : Controller
    {
        private AppDbContext db = new AppDbContext();

        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}