using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ArtFight.Models
{
    public class Comment
    {
        [Key]
        public int id { get; set; }

        public DateTime created_at { get; set; }

        [Required(ErrorMessage = "Text Required")]
        public string text { get; set; }

        // Who commented
        //public int identity_id { get; set; }


        // whom commented?
        public int participand_id { get; set; }

        
    }
}