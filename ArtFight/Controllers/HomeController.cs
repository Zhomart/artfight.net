using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArtFight.Models;

namespace ArtFight.Controllers
{
    public class HomeController : Controller
    {
        ArtfightEntities db = new ArtfightEntities();

        //
        // GET: /Home/

        public ActionResult Index()
        {
            var competitions = from s in db.Competitions
                               where s.status > 0
                               orderby s.status, s.begin descending
                               select s;
            return View(competitions);
        }

    }
}
