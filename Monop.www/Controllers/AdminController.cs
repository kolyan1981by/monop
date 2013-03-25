using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Monop.Forum;

namespace Monop.www.Controllers
{
    public class AdminController : BaseController
    {

        // GET: /Forum/
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Log(int? pg)
        {
            var db = DBService.Data;

            var p = db.Page<SiteLog>(pg.GetValueOrDefault(1), 20, "select * from SiteLog order by date desc");

            return View(p);
        }
        public ActionResult Users(int? pg)
        {
            var db = DBService.Data;

            var p = db.Page<User>(pg.GetValueOrDefault(1), 20, "select * from users order by uid");

            return View(p);
        }
      
    }
}
