using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ArtFight.Models
{
    public class Client
    {
        [Key]
        public int id { get; set; }

        [Required(ErrorMessage = "Text Required")]
        public string username { get; set; }

        [Required(ErrorMessage = "Text Required")]
        public string email { get; set; }

        [Required(ErrorMessage = "Text Required")]
        public string password { get; set; }

        public string role { get; set; }

        public string first_name { get; set; }
        public string last_name { get; set; }

        public static Client find_by_username(string username)
        {
            var db = new Context();

            var client = (from s in db.Clients
                          where s.username == username
                          select s).FirstOrDefault();
            return client;
        }

        public string fullname() {
            var s = first_name + " " + last_name;
            if (s.Length < 3) return username;
            return s;
        }

        public static Client find_by_username_and_password(string username, string password)
        {
            var db = new Context();

            var client = (from s in db.Clients
                          where s.username == username
                          where s.password == password
                          select s).FirstOrDefault();
            return client;
        }

        public static Client current_user()
        {
            var db = new  Context();

            var cu = HttpContext.Current.Request.Cookies["current_user"];
            
            if (cu != null && cu.Value != null)
            {
                return find_by_username(cu.Value);
            }

            return null;
        }


    }


}