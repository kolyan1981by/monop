using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GameLogic;
using Monop.www.Helpers;
using System.Threading;
using System.IO;

namespace Monop.www
{
	public class GameHandlers
	{
		public static string Go(Game g, string act, string userName)
		{
			string res = "";

			if (act == "ok") g.FinishStep();

			if (g.Curr.CaughtByPolice)
			{
				if (act == "policekey")
				{
					g.Curr.FreePoliceKey--;
					g.Curr.Police = 0;
					g.ToBeginRound();
				}
				else
					PlayerAction.PayToPolice(g);
			}

			if (g.State == GameState.CanBuy || g.State == GameState.CantPay)
			{
				if (act == "auc")
				{
					g.ToAuction();
					res = GameRender.RenderAuction(g, userName);
				}
				else
				{
					PlayerAction.Buy(g);
				}

			}

			if (g.State == GameState.Auction)
			{
				if (act == "auc_y")
				{
					PlayerAction.MakeAuctionYes(g, userName);
				}
				else if (act == "auc_no")
				{
					PlayerAction.MakeAuctionNo(g, userName);
				}
				//LogicBot.CheckAuctionJob(g);
			}

			if (g.State == GameState.NeedPay)
				PlayerAction.Pay(g);

			if (g.State == GameState.CantPay)
			{
				PlayerAction.Pay(g);
				if (g.Curr.IsBot)
				{
					g.Tlogp("LeaveGame", "вы банкрот и покидаете игру", "you are bankrupt");
					GameManager.LeaveGame(g, g.Curr.Name);
				}
				else
				{

					res = g.Text("go.CannotPay", "у вас нет денег, попробуйте заложить или продать <br />",
						"you dont have money,try mortage <br />");

					res += "<br />" + GameRender.UIParts("ButtonPay");
				}
				//PlayerAction.Pay(g);
			}

			if (g.State == GameState.Trade)
			{
				PlayerAction.MakeTrade(g, userName, act == "tr_y");
			}

			if (g.State == GameState.MoveToCell)
			{
				if (g.Curr.Pos == 30)
					PlayerStep.MoveFrom30(g);
				else
					PlayerStep.MoveAfterRandom(g);
			}

			if (g.State == GameState.EndStep)
			{
				g.FinishRound();
			}


			return res;
		}

		//state - p0-1-2-3-m400
	
		
		public static bool BuildOrSell(Game g, string state, string action, string userName)
		{
			//dont build if user on your land

			if (action == "build" && (g.State == GameState.NeedPay || g.State == GameState.CantPay)) return false;

			var pst = state.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

			var ss = pst.OrderByDescending(x => x.Length).Take(1).ToArray();

			Player from = g.GetPlayer(userName);

			var q = ss.Length == 1;

			if (q && from != null)
			{
				var from_ids = ss[0].Replace("p" + from.Id, "").Trim('-').Split('-').Select(x => Int32.Parse(x)).ToArray();
				if (action == "build")
					return PlayerAction.Build(g, from.Id, from_ids);
				else
				{
					PlayerAction.SellHouses(g, from.Id, from_ids);
					return true;
				}

			}
			return false;
			//g.FinishStep();
		}


		public static bool Mortgage(Game g, string state, string userName)
		{
			var pst = state.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
			Player p = g.GetPlayer(userName);

			var ss = pst.SingleOrDefault(x => x.StartsWith("p" + p.Id));
			if (ss.Trim() == "p" + p.Id) return false;

			if (ss != null && p != null)
			{
				var from_ids = ss.Replace("p" + p.Id, "").Trim('-').Split('-').Select(x => Int32.Parse(x)).ToArray();

				PlayerAction.MortgageLands(g, p.Id, from_ids);
			}
			return true;

		}




	}
}
