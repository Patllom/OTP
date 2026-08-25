using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OTT.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult objective()
        {
            return View();
        }
        public ActionResult scopeofwork()
        {
            return View();
        }
        public ActionResult eventpic()
        {
            return View();
        }
        public ActionResult knowledgetech()
        {
            return View();
        }
        public ActionResult information()
        {
            return View();
        }
        public ActionResult projectresult()
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