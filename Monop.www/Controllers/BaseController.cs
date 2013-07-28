using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using Monop.Data;
using GameLogic;
using Monop.www.Helpers;
using System.IO;
using Monop.Forum;
using System.Text;
using System.Web.UI;


namespace Monop.www.Controllers
{
	[SetCulture]
	public class BaseController : Controller
	{

		public bool IsAdmin
		{
			get
			{
				if (Request.IsAuthenticated)
				{
					var uname = User.Identity.Name;
					if (uname == Monop.www.Const.ADMIN_NAME || uname == "admin") return true;
				}
				return false;
			}

		}
		protected void LoqReq(HttpRequestBase Request)
		{
			DBService.LoqReq(Request);
		}
		public string CurrUser { get { return User.Identity.Name; } }

		public List<Game> GameContext
		{
			get
			{
				return HttpContext.Application[Const.APP_GAMES_KEY] as List<Game>;
			}
		}

		public Game GetGame()
		{
			if (Request.IsAuthenticated)
			{
				var gg = GameContext;
				return gg.FirstOrDefault(x => x.Players.Any(a => a.Name == User.Identity.Name) && !x.IsFinished);
			}
			else return null;

		}

		public Game FindGameByID(Guid id)
		{
			return GameContext.FirstOrDefault(x => x.Id == id);
		}

		public Game GetUserGame()
		{
			var gg = GameContext;
			var id = Session["gid"] as string;
			if (id != null)
				return FindGameByID(new Guid(id));
			else return null;
		}

		public Game FindGameByUserName(string userName)
		{
			var gg = GameContext;
			foreach (var g in gg)
			{
				foreach (var pl in g.Players)
				{
					if (pl.Name == userName && !g.IsFinished) return g;
				}
			}
			return null;
		}




		public ActionResult SetCulture(string id)
		{

			HttpCookie userCookie = Request.Cookies["Culture"];

			userCookie.Value = id;
			userCookie.Expires = DateTime.Now.AddYears(100);
			Response.SetCookie(userCookie);

			return Redirect(Request.UrlReferrer.ToString());
		}

		protected JsonResult RenderGame(Game g)
		{
			//return Template.RenderPartialToString("~/Partials/Map.ascx", g);
			var jsonData = new
			{
				Map = g.Cells.Select(x => new
				{
					id = x.Id,
					text = x.PrintTextOnCell,
					color = MapHelper.GetPlayerColorRGB(x.Owner),
				}),
				Players = MapHelper.GetPlayerState(g.Players).Select(x => new
				{
					id = x.Key,
					images = x.Value,
				}),
				PlayersState = RenderRazorViewToString("Game/PlayersState", g),
				GameLog = string.Join("<br />", g.LogInfo),
			};
			return Json(jsonData);
		}

		protected string RenderRazorViewToString(string viewName, object model)
		{
			ViewData.Model = model;
			using (var sw = new StringWriter())
			{
				var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
				var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
				viewResult.View.Render(viewContext, sw);
				viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
				return sw.GetStringBuilder().ToString();
			}
		}

		protected int ToInt(string p)
		{
			if (string.IsNullOrEmpty(p))
				return 0;
			return Int32.Parse(p);
		}
	}

}
