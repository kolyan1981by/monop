using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GameLogic;
using Monop.Data;
using Monop.www.Helpers;



namespace Monop.www.Controllers
{
	public class AjaxController : BaseController
	{
		Game g;

		private Game GetGame()
		{
			//get user game
			return GetUserGame();
		}

		[HttpPost]
		public string PlayerCells()
		{
			var g = GetGame();
			//var res = Template.RenderPartialToString("~/Partials/PlayerCells.ascx", g);
			var res = RenderRazorViewToString("Game/PlayerCells", g);
			return res;
		}

		[HttpPost]
		public string ShowTrades()
		{
			var g = GetGame();
			var res = "";
			foreach (var tr in g.CompletedTrades)
			{
				var giveCells = string.Join(",", tr.give_cells.Select(x => x + "").ToArray());
				var getCells = string.Join(",", tr.get_cells.Select(x => x + "").ToArray());
				res += string.Format("<br /> player {0}[give={1}]  {2} [give={3}]",
					tr.from.htmlName, giveCells, tr.to.htmlName, getCells);
			}
			return res;
		}


		[HttpPost]
		public ActionResult GameInfo()
		{
			var g = GetGame();
			if (g == null)
			{
				var jsonData1 = new
				{
					MapInfo = "",
					GameLog = "game finish",

				};
				return Json(jsonData1);
			}
			return RenderGame(g);
		}


		[HttpPost]
		public ActionResult ShowGames()
		{

			var jsonData = new
			{
				//Games = MVCHelper.RenderPartialToString("~/Views/Shared/Controls/_ShowGames.cshtml", GameContext),
				Games = RenderRazorViewToString("Controls/_ShowGames", GameContext),
				NeedPlay = NeedPlay(),
			};

			return Json(jsonData);

		}

		public string NeedPlay()
		{
			if (Request.IsAuthenticated)
			{
				var g = FindGameByUserName(CurrUser);
				if (g != null)
				{
					if (g.RunningGame && !g.IsFinished)
						return "yes";
				}
			}

			return "no";
		}


		[HttpPost]
		public ActionResult Roll(int roll)
		{
			g = GetGame();
			if (g == null) return RedirectToAction("Index", "Home");

			if (g.State == GameState.BeginStep)
			{
				if (g.IsManualMode)
				{
					var p = g.GetPlayer(CurrUser);

					if (p.ManRoll == 0)
					{
						p.ManRoll = roll;
						g.Tlog("step.ManRoll.pressed", "{0} нажал", "{0} pressed", p.htmlName);
					}
				}
				else
					if (g.IsCurrPlayer(CurrUser))
						PlayerStep.MakeStep(g);
			}

			return RenderGame(g);

		}

		[HttpPost]
		public ActionResult Go(string act)
		{
			g = GetGame();
			if (g == null) return RedirectToAction("Index", "Home");

			if (act == "exit")
			{
				GameManager.LeaveGame(g, CurrUser);
				return RedirectToAction("Index", "Home");
			}
			if (act == "InitManTrades")
			{
				SiteGameHelper.LoadResources(g, Server.MapPath(ConfigHelper.ResDir));
			}
			if (act == "SetAsBot")
			{
				var pl = g.GetPlayer(CurrUser);

				if (pl != null) pl.IsBot = !pl.IsBot;

			}

			GameHandlers.Go(g, act, CurrUser);

			return RenderGame(g);

		}


		#region PlayerCells actions

		[HttpPost]
		public ActionResult Mortgage(string str)
		{
			g = GetGame();
			var res = "";
			if (g == null) return RedirectToAction("Index", "Home");

			if (Request.IsAuthenticated)
			{
				var actRes = GameHandlers.Mortgage(g, str, CurrUser);
				if (actRes)
					res = g.Text("mort1", "вы заложили :", "you mortgage:");//+ Lands(ids);
				else
					res = "error";
			}
			else
			{
				res = "error";
			}

			return RenderLogAndPlayerCells(g);

		}

		[HttpPost]
		public ActionResult Trade(string str)
		{
			g = GetGame();
			var res = "";
			if (g != null && g.State == GameState.BeginStep)
			{
				g.SetState(GameState.Trade);
				GameManager.InitTrade(g, str, CurrUser);
				//GameManager.CheckTradeJob(g);
			}

			return RenderLogAndPlayerCells(g);

		}

		[HttpPost]
		public ActionResult Build(string str)
		{
			g = GetGame();
			var res = "";

			if (Request.IsAuthenticated)
			{
				var actRes = GameHandlers.BuildOrSell(g, str, "build", CurrUser);
				if (actRes)
					res = string.Format("{0} вы построили :", g.Curr.htmlName);
				else
					res = string.Format("{0} не хватает денег:", g.Curr.htmlName);
			}
			else
			{
				res = "error";
			}

			var jsonData = new
			{
				//MapInfo = RenderGame(g),
				GameLog = string.Join("<br />", g.LogInfo),
				PlayerCells = RenderRazorViewToString("Game/PlayerCells", g),

			};
			return RenderLogAndPlayerCells(g);

		}

		[HttpPost]
		public ActionResult SellHouses(string str)
		{
			g = GetGame();
			if (Request.IsAuthenticated)
			{
				var actRes = GameHandlers.BuildOrSell(g, str, "sell", CurrUser);
			}

			return RenderLogAndPlayerCells(g);
		}

		private ActionResult RenderLogAndPlayerCells(Game g)
		{
			var jsonData = new
			{
				GameLog = string.Join("<br />", g.LogInfo),
				PlayerCells = RenderRazorViewToString("Game/PlayerCells", g),

			};
			return Json(jsonData);
		}

		#endregion

		public string RunCommand(string comm)
		{
			g = GetGame();
			if (g != null)
				GameLogic.GameCommands.Run(g, comm);

			return "";
		}
		#region Chat

		public string SendMessage(string mes)
		{
			var ll = HttpContext.Application["chat"] as List<String>;


			string user = "";
			if (Request.IsAuthenticated)
			{
				user = CurrUser;
			}
			var hh = DateTime.Now.ToString("hh:mm:ss") + " : " + user;

			if (ll != null)
			{
				ll.Add(hh + " : " + mes);
				if (ll.Count > 10) ll.RemoveAt(0);
			}
			return ShowChat();
		}

		public string SendGameMessage(string mes)
		{
			if (!string.IsNullOrEmpty(mes))
				if (Request.IsAuthenticated)
				{
					var g = FindGameByUserName(CurrUser);
					if (g != null)
					{
						var pl = g.GetPlayer(CurrUser);
						var hh = pl.htmlName + "says";

						var ll = g.Logs;

						if (ll != null)
						{
							var smilesMes = ImageHelper.ProcessMessage(mes);
							ll.Add(hh + " : " + smilesMes);
						}
					}
				}

			return "";

		}

		public string ShowChat()
		{
			var ll = HttpContext.Application["chat"] as List<String>;
			var res = "";
			foreach (var item in ll)
			{
				res += string.Format("{0}<br />", item);
			}
			return res;
		}

		#endregion


		#region Site translation

		[AcceptVerbs(HttpVerbs.Post)]
		public void TranslateText(string id, string textEN, string textRU)
		{
			SessionHelper.AddOrUpdateCachedText(id, textEN, textRU);
		}

		[AcceptVerbs(HttpVerbs.Get)]
		public JsonResult GetText(string id)
		{
			var tt = SessionHelper.GetCachedText(id);

			return Json(new { en = tt[0], ru = tt[1] }, JsonRequestBehavior.AllowGet);
		}
		#endregion

	}
}
