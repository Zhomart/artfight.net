using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ArtFight.Models
{
    public class Competition
    {
        [Key]
        public int id { get; set; }

        [DisplayName("Title")]
        [Required(ErrorMessage = "Title Required")]
        public string title { get; set; }

        public string description { get; set; }

        public DateTime begin { get; set; }
        public DateTime end { get; set; }

        public int status { get; set; }

        // who created this competition
        [ScaffoldColumn(false)]
        public string owner_username { get; set; }

        public List<Participant> participants()
        {
            var db = new Context();
            var ps = from s in db.Participants
                     where s.competition_id == this.id
                     orderby s.likes descending
                     select s;
            return ps.ToList();
        }

        public void update_status() {
            DateTime now = DateTime.Now;

            if (status < 3 && now > end)
            {
                status = 3;
            }

            if (status == 0 && now > begin) 
            {
                status = 1;
            }

            var db = new Context();
            db.Entry(this).State = System.Data.EntityState.Modified;
            db.SaveChanges();
        }

        public bool participating(string username) 
        {
            var db = new Context();
            var ps = from s in db.Participants
                     where s.competition_id == this.id
                     where s.username == username
                     select s;
            //throw new Exception(username);
            return ps.Count() > 0;
        }

        // other codes

        public static string[] statuses = new string[4] { "Not Started", "Running", "Suspended", "Finished" };

        [NotMapped]
        public string status_name { get { return statuses[status]; } }
    }
}