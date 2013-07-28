using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using GameLogic;
using Monop.www.Helpers;
using Monop.Data;
using System.Text;

namespace Monop.www.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {

            ViewData["Message_Error_NewGame"] = "you already created game";

            ViewBag.Admin = IsAdmin;

            if (ConfigHelper.EnableDB)
                ViewBag.TopPlayers = Monop.Data.GameRepo.GetTop();
            else ViewBag.TopPlayers = new[] { "admin wins 1" };

            if (!Request.IsAuthenticated)
            {
                var userName = "user" + DateTime.Now.ToString("mmss");
                FormsAuthentication.SetAuthCookie(userName, true);
            }

            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult NewGame(int Count, int timeout, int rollmode, bool? debug)
        {
            if (Request.IsAuthenticated)
            {
                var uname = User.Identity.Name;

                var yourGames = FindGameByUserName(uname);
                if (yourGames == null)
                {
                    var g = GameHelper.CreateNew(Count, timeout, 
                        SessionHelper.Locale, rollmode, debug.GetValueOrDefault(false),2000);

                    Session["gid"] = g.Id.ToString();
                    
                    g.Players.Add(new Player { Id = 0, Name = uname, Status = GetStatus(uname), Money = Const.START_CASH });
                    
                    //g.OnLeavedGame += (s, w) => { GameAction.SaveToDB(s, g, w); };
                    g.SetLeaveGameAction((s, w) => { SiteGameHelper.SaveToDB(s, g, w); });

                    SiteGameHelper.LoadResources(g, Server.MapPath(ConfigHelper.ResDir));

                    GameContext.Add(g);
                }
                {
                    ViewData["Message_Error_NewGame"] = "you already created game";
                    return RedirectToAction("Index");
                }
            }

            return RedirectToAction("LogOn", "Account");

        }

        public ActionResult Play()
        {
            if (Request.IsAuthenticated)
            {
                var uname = User.Identity.Name;

                var g = FindGameByUserName(uname);
                if (g != null)
                {
                    Session["gid"] = g.Id.ToString();
                    g.StartGame();

                    return RedirectToAction("Play", "Game");
                }
            }
            else
            {
                RedirectToAction("LogOn", "Account");
            }

            return RedirectToAction("Index");
        }




        public ActionResult Join(Guid id)
        {

            if (Request.IsAuthenticated)
            {
                var uname = User.Identity.Name;


                var yourGames = FindGameByUserName(uname);
                if (yourGames == null)
                {
                    Session["gid"] = id.ToString();

                    var g = GetUserGame();
                    if (g != null)
                    {
                        var count = g.Players.Count;
                        if (count < g.conf.cnfMaxPlayers)
                        {
                            g.Players.Add(new Player
                            {
                                Id = count,
                                Name = uname,
                                Status = GetStatus(uname),
                                Money = 15000000
                            });
							if (g.Players.Count == g.conf.cnfMaxPlayers)
                            {
                                g.StartGame();

                                return RedirectToAction("Play", "Game");
                            }
                        }
                        else
                            return RedirectToAction("Index");
                    }
                }
            }
            else
            {
                RedirectToAction("LogOn", "Account");
            }

            return RedirectToAction("Index");
        }

        public ActionResult AddBot(Guid id)
        {

            if (Request.IsAuthenticated)
            {
                var uname1 = User.Identity.Name;

                var g = FindGameByID(id);
                if (g != null)
                {
                    //Session["gid"] = id.ToString();
                    
                    var plCount = g.Players.Count;
                    if (plCount < g.conf.cnfMaxPlayers && g.Players[0].Name == uname1)
                    {
                        var uname = "comp" + plCount;
                        var pl = new Player { Id = plCount, Name = uname, IsBot = true, Status = GetStatus(uname), Money = Const.START_CASH };
                        g.Players.Add(pl);
                        if (g.Players.Count == g.conf.cnfMaxPlayers)
                        {
                            g.StartGame();
                            return RedirectToAction("Play", "Game");
                        }

                    }
                    return RedirectToAction("Index");

                }
            }
            else
            {
                RedirectToAction("LogOn", "Account");
            }

            return RedirectToAction("Index");
        }

        private static string GetStatus(string uname)
        {
            if (ConfigHelper.EnableDB)
                return GameRepo.GetPlayerStatus(uname);
            else
                return "new";
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult RestoreGame(FormCollection forms)
        {
            string upload_dir = Server.MapPath("~/upload/");

            string fileState = "";
            foreach (string f in Request.Files.Keys)
            {
                if (Request.Files[f].ContentLength > 0)
                {
                    //Request.Files[f].SaveAs(upload_dir + Path.GetFileName(Request.Files[f].FileName));
                    var ff = Request.Files[f];

                    byte[] buf = new byte[ff.ContentLength];

                    ff.InputStream.Read(buf, 0, ff.ContentLength);

                    fileState = Encoding.UTF8.GetString(buf, 0, buf.Length);
                }
            }


            if (Request.IsAuthenticated)
            {
                var g = GameHelper.RestoreGameFromXML(fileState);
                //g.DebugMode = true;

                g.SetLeaveGameAction((s, w) => { SiteGameHelper.SaveToDB(s, g, w); });
                SiteGameHelper.LoadResources(g, Server.MapPath(ConfigHelper.ResDir));

                Session["gid"] = g.Id.ToString();

                GameContext.Add(g);
                g.StartGame();
                return RedirectToAction("Index");
            }

            return RedirectToAction("LogOn", "Account");

        }


        public ActionResult Delete(Guid id)
        {

            if (Request.IsAuthenticated)
            {
                var uname = User.Identity.Name;

                var gg = GameContext;
                Game g = gg.FirstOrDefault(x => x.Id == id);

                if (g != null)
                {
                    if (g.pcount > 0)
                    {
						if (g.IsGameCreator(uname) || ConfigHelper.IsAdmin(uname))
                        {
                            gg.Remove(g);
                        }
                    }
                    else gg.Remove(g);
                }
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("LogOn", "Account");
            }


        }


        public ActionResult Dialog()
        {
            return View();
        }


        public ActionResult About()
        {
            return View();
        }
        public ActionResult MapView()
        {
            var g = GetGame();
            return View(g);
        }
    }
}
