using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ArtFight.Models
{
    public class Context : DbContext
    {
        public DbSet<Competition> Competitions { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Participant> Participants { get; set; }

    }
}