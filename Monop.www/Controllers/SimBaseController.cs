using GameLogic;
using Monop.www.GameHelpers;
using Monop.www.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Monop.www.Controllers
{
	public class SimBaseController : BaseController
	{
		protected JsonResult RenderRound(int r, Game g)
		{
			var list = g.RoundActions.Where(x => x.round == r).ToList();

			var ra = list.Any() ? list.Last() : 
				new GameAction { Cells = g.Cells, Players = g.Players };

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
				PlayersInfo = SimHelper.GetPlayersInfo(ra),
				PlayersState = RenderRazorViewToString("Game/PlayersState", g),
				Round = r,
				MaxRound = g.RoundNumber
			};
			return base.Json(data);
		}

	}
}
