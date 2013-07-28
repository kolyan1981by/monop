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
	public class SimController : SimBaseController
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

		
		
	
	}
}
