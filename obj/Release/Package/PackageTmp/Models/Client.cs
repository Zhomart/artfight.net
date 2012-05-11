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

        public static Client find_by_username(Context db, string username)
        {
            var client = (from s in db.Clients
                          where s.username == username
                          select s).FirstOrDefault();
            return client;
        }

        public string fullname() {
            if ((first_name + " " + last_name).Length == 0) return username;
            return first_name + " " + last_name;  
        }

        public static Client find_by_username_and_password(Context db, string username, string password)
        {
            var client = (from s in db.Clients
                          where s.username == username
                          where s.password == password
                          select s).FirstOrDefault();
            return client;
        }

        public static Client current_user(Context db)
        {
            var cu = HttpContext.Current.Request.Cookies["current_user"];
            
            if (cu != null && cu.Value != null)
            {
                return find_by_username(db, cu.Value);
            }

            return null;
        }


    }


}