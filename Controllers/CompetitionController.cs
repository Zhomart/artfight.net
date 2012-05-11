using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArtFight.Models;
using System.IO;
using System.Net;
using System.Collections.Specialized;


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

        public static void HttpUploadFile(string url, Stream input, string paramName, string fileName, string contentType, NameValueCollection nvc)
        {
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
            wr.ContentType = "multipart/form-data; boundary=" + boundary;
            wr.Method = "POST";
            wr.KeepAlive = true;
            wr.Credentials = System.Net.CredentialCache.DefaultCredentials;

            Stream rs = wr.GetRequestStream();

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            string header = string.Format(headerTemplate, paramName, fileName, contentType);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            rs.Write(headerbytes, 0, headerbytes.Length);

            byte[] buffer = new byte[4096];
            int bytesRead = 0;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) != 0)
            {
                rs.Write(buffer, 0, bytesRead);
            }

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            WebResponse wresp = null;
            try
            {
                wresp = wr.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                //throw new Exception(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
            }
            catch (Exception ex)
            {
                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
                throw new Exception("Error uploading file", ex);
            }
            finally
            {
                wr = null;
            }
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

                string picture_host = "http://92.46.55.99:9008";
                // saving picture on other server
                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("id", "TTR");
                nvc.Add("btn-submit-photo", "Upload");
                HttpUploadFile(picture_host + "/upload.php", picture.InputStream, "file", fileName, picture.ContentType, nvc);

                var p = new Participant { description = pm.desciption, likes = 0, picture_url = picture_host + "/uploads/" + fileName, username = username, competition_id = c.id };
                
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

            var comments = from s in db.Comments
                           where s.participand_id == participant_id
                           orderby s.created_at descending
                           select s;

            @ViewBag.comments = comments;

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
        public string Comment(int id)
        {
            var c = db.Competitions.Find(id);

            var text = Request.Params["text"];

            int participant_id = int.Parse(Request.Params["participant_id"]);

            var participant = db.Participants.Find(participant_id);

            var client = Client.current_user();

            var comment = new Comment { created_at = DateTime.Now, participand_id = participant.id, text = text, client_id = client.id };

            db.Comments.Add(comment);
            db.SaveChanges();

            return "ok";
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