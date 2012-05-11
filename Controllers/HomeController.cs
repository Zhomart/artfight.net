using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArtFight.Models;
using System.Configuration;

namespace ArtFight.Controllers
{
    public class HomeController : ApplicationController
    {

        //
        // GET: /Home/

        public ActionResult Index()
        {
            //var competitions = from s in db.Competitions
            //                   where s.status > 0
            //                   orderby s.status, s.begin descending
            //                   select s;
            var competitions = new List<Competition>();
            ViewBag.s = ConfigurationManager.ConnectionStrings["context"];
            return View(competitions);
        }

    }
}
