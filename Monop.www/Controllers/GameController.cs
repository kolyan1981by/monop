using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GameLogic;
using Monop.www.Helpers;
using Monop.www;

namespace Monop.www.Controllers
{
    public class GameController : BaseController
    {
        //
        // GET: /Game/
        Game g;

        public ActionResult Index()
        {
            g = GetGame();
            if (Request.IsAuthenticated)
            {
                ViewData["admin"] = IsAdmin;


                if (g != null)
                {
                    ViewData["game"] = g;
                    return View();
                }
                else return RedirectToAction("Index", "Home");
            }
            else
                return RedirectToAction("LogOn", "Account");

            return View();
        }

        public ActionResult Play()
        {
            g = GetGame();
            if (Request.IsAuthenticated)
            {
                ViewData["admin"] = IsAdmin;

                if (g != null)
                {
                    ViewData["rollMode"] = g.conf.cnfRollMode;
                    Session["gid"] = g.Id.ToString();
                    return View();
                }
                else return RedirectToAction("Index", "Home");
            }
            else
                return RedirectToAction("LogOn", "Account");
        }

        [HttpGet]
        public ActionResult View(Guid id)
        {
            Session["gid"] = id.ToString();
            g = GetUserGame();
            if (g != null)
            {
                return View();
            }
            else return RedirectToAction("Index", "Home");
        }


    

        #region Admin section

        [AcceptVerbs(HttpVerbs.Get)]
        public ActionResult Store()
        {
            var g = GetUserGame();
            if (g != null)
            {
                Response.Clear();
                Response.ContentType = "text/plain";
                Response.AddHeader("Content-Disposition", string.Format("attachment; filename=state.xml"));
                Response.Write(GameHelper.RenderLastStateXML(g));
                Response.End();
                return View();
            }
            else return RedirectToAction("NewGame");
        }

        [HttpPost]
        public ActionResult Next()
        {
            g = GetGame();

            if (g != null)
            {
                //GameManager.CheckState(g);
                PlayerStep.MakeStep(g);
            }

            return RenderGame(g); ;
        }

        [HttpPost]
        public string LeaveGame()
        {
            g = GetGame();

            if (g != null)
            {
                GameManager.LeaveGame(g, CurrUser);

                //Monop.Data.DataManager.SaveState(CurrUser, g.LastGameState,false);
            }
            return "";

        }

        #endregion
    }
}
