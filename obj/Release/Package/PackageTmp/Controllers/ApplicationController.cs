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

            ViewBag.current_client = Client.current_user(db);
            ViewBag.is_authorized = ViewBag.current_client != null;

            //throw new Exception("a "+ViewBag.current_client.username);
        }


    }
}
