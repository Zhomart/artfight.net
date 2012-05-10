using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArtFight.Models;
using System.IO;


namespace ArtFight.Controllers
{ 
    public class CompetitionController : Controller
    {
        private ArtfightEntities db = new ArtfightEntities();


        //
        // GET: /Competition/
        public ActionResult Participate(int id)
        {
            var c = (from s in db.Competitions
                    where s.id == id
                    select s).First();

            var p = new ParticipateModel();

            p.competition_id = c.id;

            return View(p);
        }

        //
        // POST: /Competition/Create
        [HttpPost]
        public ActionResult Participate(ParticipateModel pm, HttpPostedFileBase picture)
        {

            if (ModelState.IsValid && picture != null && picture.ContentLength > 0)
            {
                var c = (from s in db.Competitions
                         where s.id == pm.competition_id
                         select s).First();

                // Current logined username
                string username = User.Identity.Name;

                // extract only the extension, and generate random string with length 24
                var fileName = Helper.RandomString(24) + Path.GetExtension(picture.FileName);
                // store the picture inside ~/Public/participant_pictures folder
                var path = Path.Combine(Server.MapPath("~/Public/participant_pictures"), fileName);
                picture.SaveAs(path);

                var p = new Participant { description = pm.desciption, likes = 0, picture_url = "Public/participant_pictures/" + fileName, username = username, competition_id = c.id };
                
                db.Participants.Add(p);
                db.SaveChanges();

                return RedirectToAction("Index");
            }

            return View(pm);
        }
        

        //
        // GET: /Competition/

        public ViewResult Index()
        {
            var competitions = from s in db.Competitions
                               where s.status > 0
                               orderby s.status
                               select s;

            return View(competitions);
        }

        //
        // GET: /Competition/

        public ViewResult My()
        {
            string username = User.Identity.Name;

            var competitions = from s in db.Competitions
                               where s.owner_username == username
                               select s;

            return View(competitions);
        }

        //
        // GET: /Competition/Details/5

        public ViewResult Details(int id)
        {
            Competition competition = db.Competitions.Find(id);
            return View(competition);
        }

        //
        // GET: /Competition/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Competition/Create

        [HttpPost]
        public ActionResult Create(Competition competition)
        {
            if (ModelState.IsValid)
            {
                string username = User.Identity.Name;
                competition.owner_username = username;
                db.Competitions.Add(competition);
                db.SaveChanges();
                return RedirectToAction("Index");  
            }

            return View(competition);
        }
        
        //
        // GET: /Competition/Edit/5
 
        public ActionResult Edit(int id)
        {
            Competition competition = db.Competitions.Find(id);
            return View(competition);
        }

        //
        // POST: /Competition/Edit/5

        [HttpPost]
        public ActionResult Edit(Competition competition)
        {
            string username = User.Identity.Name;
            if (competition.owner_username == username)
            {
                if (ModelState.IsValid)
                {
                    db.Entry(competition).State = EntityState.Modified;
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            else 
            {
                ViewBag.error = "You don't have permission to do this.";
            }
            return View(competition);
        }

        //
        // GET: /Competition/Delete/5
 
        public ActionResult Delete(int id)
        {
            Competition competition = db.Competitions.Find(id);
            return View(competition);
        }

        //
        // POST: /Competition/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            string username = User.Identity.Name;

            Competition competition = db.Competitions.Find(id);
            if (competition.owner_username == username)
            {
                db.Competitions.Remove(competition);
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                ViewBag.error = "You don't have permission to do this.";
            }

            return View(competition);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}