using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace ArtFight.Models
{
    public class Participant
    {
        [Key]
        public int id { get; set; }

        public int likes { get; set; }

        public string picture_url { get; set; }

        public string description { get; set; }

        public int competition_id { get; set; }

        // Who participates
        public string username { get; set; }

        // Comments commented to this participant
        // public List<Comment> comments { get; set; }

    }

    public class ParticipateModel
    {
        [DataType(DataType.Text)]
        [Display(Name = "Description")]
        public string desciption { get; set; }

        //[Required]
        //[Display(Name = "Picture")]
        //[UIHint("Picture XD")]
        //public string picture_url { get; set; }

        [ScaffoldColumn(false)]
        public int competition_id { get; set; }
    }
}