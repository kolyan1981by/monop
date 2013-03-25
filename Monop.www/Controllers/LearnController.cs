using GameLogic;
using Monop.www.GameHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Monop.www.Controllers
{
    public class LearnController : Controller
    {
        //
        // GET: /Learn/

        public ActionResult Index()
        {
            return View();
        }

        #region auc

        public ActionResult Auc(string act)
        {
            return View(GetRules);
        }

        public ActionResult SaveAucRules()
        {
            SimHelper.SaveAucRules(GetRules);
            Session["auc_list"] = null;

            return RedirectToAction("Auc");
        }

        public ActionResult AddAuc(int gr_id, int myc, int ac, double fac, string nb)
        {
            var list = GetRules;
            var rec = new AucRule();// list.First(x => x.Id == rid);
            rec.Id = list.Max(x => x.Id) + 1;
            rec.GroupId = gr_id;
            rec.MyCount = myc;
            rec.AnCount = ac;
            rec.Factor = fac;
            rec.GroupsWithHouses = nb;
            list.Add(rec);
            return RedirectToAction("Auc");
        }

        public string EditAuc(int rid, int myc, int ac, double fac, string nb)
        {
            var list = GetRules;
            var rec = list.First(x => x.Id == rid);
            rec.MyCount = myc;
            rec.AnCount = ac;
            rec.Factor = fac;
            rec.GroupsWithHouses = nb;

            return "successfull";
        }

        public string DelAuc(int rid)
        {
            var list = GetRules;
            var rec = list.First(x => x.Id == rid);
            list.Remove(rec);
            return "successfull";
        }

        public List<GameLogic.AucRule> GetRules
        {
            get
            {
                var list = Session["auc_list"] as List<GameLogic.AucRule>;
                if (list == null)
                    Session["auc_list"] = list = ManualLogicHelper.LoadAucRules(SimHelper.ReadFromFile().ToArray()).ToList();
                return list;

            }
        }

        public ActionResult TestRules()
        {
            //ViewBag.Result = "test";
            return View(GetRules);
        }
        [HttpPost]
        public ActionResult TestRules(int gr_id, int myc, int ac, string nb)
        {
            var groups = nb.Split(',')
         .Select(x => Int32.Parse(x)).ToArray();

            var factor = BotBrain.GetManualFactor(GetRules.Where(x => x.GroupId == gr_id), gr_id, myc, ac, groups);
            
            ViewBag.Result = "factor=" + factor;
            
            return View(GetRules);
        }
        #endregion
    }
}
