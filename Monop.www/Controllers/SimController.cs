using GameLogic;
using Monop.www.GameHelpers;
using Monop.www.Helpers;
using Monop.www.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Monop.www.Controllers
{
    public class SimController : Controller
    {

        public ActionResult Index(string act)
        {
            if (act == "sim")
            {
                Game game = Simulator.Run(Server.MapPath("~/res"));
                Session["sim_game"] = game;
                ViewBag.Log = SimHelper.ShowActions(game);
                ViewBag.MaxRound = game.RoundActions.Max(x => x.round);
                ViewBag.ChartData = SimHelper.GetChartData(game);
            }

            return View();
        }

        [HttpPost]
        public ActionResult Round(int? r)
        {
            if (!r.HasValue || r < 0)
            {
                r = new int?(0);
            }
            Game game = base.Session["sim_game"] as Game;
            if (game == null)
            {
                var data = new
                {
                    Map = "",
                    GameLog = "game finish"
                };
                return base.Json(data);
            }
            return this.RenderRound(r.Value, game);
        }

        public JsonResult RenderRound(int r, Game g)
        {
            var list = g.RoundActions.Where(x => x.round == r).ToList();

            var ra = list.Last();

            var data = new
            {
                Map =
                    from x in ra.Cells
                    select new
                    {
                        id = x.Id,
                        text = x.PrintTextOnCell,
                        color = MapHelper.GetPlayerColorRGB(x.Owner)
                    },
                Players =
                    from x in MapHelper.GetPlayerState(ra.Players)
                    select new
                    {
                        id = x.Key,
                        images = x.Value
                    },
                State = "finished",
                GameLog = SimHelper.RoundLog(r, list).text,
                PlayersState = SimHelper.GetPlayersInfo(ra)
            };
            return base.Json(data);
        }


        

    }
}
