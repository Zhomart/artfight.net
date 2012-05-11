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
    public class CompetitionController : ApplicationController
    {
        //
        // GET: /Competition/
        public ActionResult Participate(int id)
        {
            var c = (from s in db.Competitions
                    where s.id == id
                    select s).First();

            c.update_status();

            if (c.end < DateTime.Now) throw new Exception("Competition finished");

            ViewBag.competition = c;

            var p = new ParticipateModel();

            p.competition_id = c.id;

            ViewBag.competition = c;

            return View(p);
        }

        //
        // POST: /Competition/Create
        [HttpPost]
        public ActionResult Participate(ParticipateModel pm, HttpPostedFileBase picture)
        {
            var c = (from s in db.Competitions
                     where s.id == pm.competition_id
                     select s).First();

            ViewBag.competition = c;

            if (ModelState.IsValid && picture != null && picture.ContentLength > 0)
            {

                var current_client = Client.current_user();

                // Current logined username
                string username = current_client.username;

                // extract only the extension, and generate random string with length 24
                var fileName = Helper.RandomString(24) + Path.GetExtension(picture.FileName);
                // store the picture inside ~/Public/participant_pictures folder
                var path = Path.Combine(Server.MapPath("~/Public/participant_pictures"), fileName);
                picture.SaveAs(path);

                var p = new Participant { description = pm.desciption, likes = 0, picture_url = "Public/participant_pictures/" + fileName, username = username, competition_id = c.id };
                
                db.Participants.Add(p);
                db.SaveChanges();

                return Redirect("/Competition/Details/" + c.id);
            }

            return View(pm);
        }

        public ViewResult Participants(int id)
        {
            int participant_id = int.Parse(Request.Params["participant_id"]);

            var competition = db.Competitions.Find(id);
            competition.update_status();

            ViewBag.competition = competition;

            var participant = (from s in db.Participants
                               where s.competition_id == competition.id && participant_id == s.id
                              select s).First();

            object a = Session[competition.id + "-" + participant.id];

            if (a == null)
            {
                @ViewBag.disabled = "";
            }
            else {
                @ViewBag.disabled = "disabled";
            }

            return View(participant);
        }

        [HttpPost]
        public string Like(int id) {
            int participant_id = int.Parse(Request.Params["participant_id"]);

            var competition = db.Competitions.Find(id);
            competition.update_status();

            var participant = (from s in db.Participants
                               where s.competition_id == competition.id && participant_id == s.id
                               select s).First();

            object a = Session[competition.id + "-" + participant.id];
            
            if (a != null) return "voted";

            Session[competition.id + "-" + participant.id] = "voted";

            if (competition.end < DateTime.Now) throw new Exception("Competition finished");

            participant.likes++;

            db.Entry(participant).State = EntityState.Modified;
            db.SaveChanges();

            return participant.likes + "";
        }

        //
        // GET: /Competition/

        public ViewResult Index()
        {
            var raw_competitions = from s in db.Competitions
                               where s.status > 0
                               orderby s.status
                               select s;

            foreach (var c in raw_competitions) c.update_status();

            var competitions = (from s in raw_competitions
                           where s.status > 0
                           select s).ToList();
           
            return View(competitions);
        }

        //
        // GET: /Competition/

        public ViewResult My()
        {
            var current_client = Client.current_user();

            // Current logined username
            string username = current_client.username;

            var raw_competitions = from s in db.Competitions
                                   where s.owner_username == current_client.username
                                   orderby s.status
                                   select s;

            foreach (var c in raw_competitions) c.update_status();

            var competitions = (from s in raw_competitions
                                where s.status > 0
                                select s).ToList();

            return View(competitions);
        }

        //
        // GET: /Competition/Details/5

        public ViewResult Details(int id)
        {
            Competition competition = db.Competitions.Find(id);
            competition.update_status();
            ViewBag.competition = competition;
            return View(competition);
        }

        //
        // GET: /Competition/Create

        public ActionResult Create()
        {
            var c = new Competition();

            c.begin = DateTime.Now;
            c.end = DateTime.Now.AddHours(24);

            return View(c);
        } 

        //
        // POST: /Competition/Create

        [HttpPost]
        public ActionResult Create(Competition competition)
        {
            if (ModelState.IsValid)
            {
                var current_client = Client.current_user();

                // Current logined username
                string username = current_client.username;

                competition.owner_username = username;
                
                competition.status = 0;

                db.Competitions.Add(competition);
                db.SaveChanges();
                return RedirectToAction("My");  
            }

            return View(competition);
        }
        
        //
        // GET: /Competition/Edit/5
 
        public ActionResult Edit(int id)
        {
            Competition competition = db.Competitions.Find(id);

            ViewBag.competition = competition;

            return View(competition);
        }

        //
        // POST: /Competition/Edit/5

        [HttpPost]
        public ActionResult Edit(Competition competition)
        {
            var current_client = Client.current_user();

            ViewBag.competition = competition;

            // Current logined username
            string username = current_client.username;

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

            ViewBag.competition = competition;

            return View(competition);
        }

        //
        // POST: /Competition/Delete/5

        [HttpPost, ActionName("Delete")]
        public ActionResult DeleteConfirmed(int id)
        {
            var current_client = Client.current_user();

            // Current logined username
            string username = current_client.username;


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