using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProductionMonitor.Controllers
{
    public class StaticController : Controller
    {
        // GET: Static
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LogoFt()
        {
            return View();
        }

        public ActionResult Logo1000()
        {
            return View();
        }
    }
}