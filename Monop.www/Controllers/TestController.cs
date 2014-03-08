using GameLogic;
using Monop.www.GameHelpers;
using Monop.www.Helpers;
using Monop.www.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace Monop.www.Controllers
{
	public class TestController : SimBaseController
	{

		public ActionResult Index()
		{
			TestVM model = new TestVM();
			model.Players = new List<TestPlayer>();
			model.Players.Add(new TestPlayer { p0m = "15000", p0p = "0", p0h = true });
			model.Players.Add(new TestPlayer { p0m = "15000", p0p = "0", p0h = false });
			CreateGame(model);

			return View(model);
		}
		[HttpPost]
		public ActionResult Index(TestVM model)
		{

			if (!string.IsNullOrEmpty(Request.Params["init"]))
				CreateGame(model);

			return View(model);
		}

		private void CreateGame(TestVM model)
		{
			var g = GameHelper.CreateNew(2, 30, "en-US", 1, model.isDebug);
			//Session["test_game"] = g;
			Session["gid"] = g.Id.ToString();
			GameContext.Clear();
			GameContext.Add(g);

			Simulator.AddPlayers(g, 2, new[] { CurrUser, "bot1" });
			Simulator.InitWithData(g, Server.MapPath("~/res"));
			g.StartGame();

			int i = 0;
			foreach (var pl in model.Players)
			{
				g.Players[i].Money = ToInt(pl.p0m) * 1000;
				g.Players[i].IsBot = !pl.p0h;
				g.Players[i].Pos = ToInt(pl.p0p);
				if (!string.IsNullOrEmpty(pl.p0c))
				{
					var cells = pl.p0c.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).ToList();
					cells.ForEach(x => g.Cells[Int32.Parse(x)].Owner = 0);
				}
				i++;
			}

			g.Map.UpdateMap();
		}

		public ActionResult Round(int? r)
		{
			var g = GetUserGame();
			var maxr = g.RoundNumber;

			var rr = r.HasValue ? r.Value : maxr;
			if (rr < 0) rr = 0;
			if (rr > maxr) rr = maxr;

			return RenderRound(rr, g);

		}
		public string MyProperty { get; set; }
		public ActionResult NextStep(int roll)
		{
			var g = FindGameByID(new Guid((string)Session["gid"]));

			//GameManager.MakeRollFrom(g, 1, roll);
			GameManager.MakeRollFrom(g, 3, GameManager.RNumber);

			PlayerStep.MakeStep(g, false);

			GameManager.CheckState(g);

			return RenderRound(g.RoundNumber - 1, g);
		}

		public ActionResult List(Guid? games)
		{
			var m = GameContext;
			if (!string.IsNullOrEmpty(Request.Params["reload"]))
				foreach (string f in Request.Files.Keys)
				{
					if (Request.Files[f].ContentLength > 0)
					{
						using (var ms = new MemoryStream())
						{
							Request.Files[f].InputStream.CopyTo(ms);
							byte[] buf = ms.GetBuffer();
							var ftext = Encoding.UTF8.GetString(buf, 0, buf.Length);
							ReloadGame(ftext);
						}
					}
				}
			if (!string.IsNullOrEmpty(Request.Params["store"]))
			{
				var g = FindGameByID(games.Value);
				if (g != null)
				{
					var gtext = GameHelper.RenderLastStateXML(g);
					var byteArray = Encoding.UTF8.GetBytes(gtext);
					var stream = new MemoryStream(byteArray);

					var fname = string.Format("state_{0}.xml", g.GameName);
					return File(stream, "text/plain", fname);
				}
			}
			return View(m);
		}

		private void ReloadGame(string ftext)
		{
			var g = GameHelper.RestoreGameFromXML(ftext);

			//g.SetLeaveGameAction((s, w) => { SiteGameHelper.SaveToDB(s, g, w); });
			SiteGameHelper.LoadResources(g, Server.MapPath(ConfigHelper.ResDir));

			var i = GameContext.FindIndex(x => x.Id == g.Id);
			GameContext[i] = g;
			g.StartGame();
		}
	}
}
