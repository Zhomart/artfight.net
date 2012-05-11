using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ArtFight.Models;

namespace ArtFight.Controllers
{
    public class ApplicationController : Controller
    {
        protected Context db = new Context();

        protected override void OnActionExecuting(ActionExecutingContext ctx)
        {
            base.OnActionExecuting(ctx);
            ctx.HttpContext.Trace.Write("Log: OnActionExecuting",
                 "Calling " +
                 ctx.ActionDescriptor.ActionName);

            ViewBag.current_client = Client.current_user();
            ViewBag.is_authorized = ViewBag.current_client != null;

            var participants = (from p in db.Participants
                    from c in db.Competitions
                    where c.status == 1 && p.competition_id == c.id
                    select p).ToList();

            ViewBag.vip_participants = participants.GetRange(0, Math.Min(8, participants.Count));

            //throw new Exception("a "+ViewBag.current_client.username);
        }


    }
}
