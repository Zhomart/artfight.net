using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace ArtFight.Models
{
    public class SampleData : DropCreateDatabaseAlways<Context>
    {
        protected override void Seed(Context context)
        {
            new List<Client>
            {
                new Client { username = "admin", password = "admin", email = "admin@gmail.com", role = "admin" },
                new Client { username = "moder", password = "moder", email = "moder@gmail.com", role = "moder" },
                new Client { username = "user", password = "user", email = "user@gmail.com", role = "" }
            }.ForEach(a => context.Clients.Add(a));

            new List<Competition>
            {
                new Competition { title="Anime art", begin = DateTime.Now, end = DateTime.Now.Add(TimeSpan.FromHours(5)), description = "blablabla", status = 1, owner_username="miku" },
                new Competition { title="Movie art 2", begin = DateTime.Now.Add(TimeSpan.FromHours(7)), end = DateTime.Now.Add(TimeSpan.FromHours(8)), status = 0, owner_username="zhomart" }
            }.ForEach(a => context.Competitions.Add(a));
        }
    }
}