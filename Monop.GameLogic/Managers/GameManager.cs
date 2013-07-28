using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml.Linq;

namespace GameLogic
{
	public static class GameManager
	{

		#region Make roll

		public static void MakeRoll(Game g)
		{
			//make step
			g.LastRoll = g.IsManualMode ? ManualRolls(g) : RandomRolls();
			g.Curr.LastRoll = g.LastRoll;
			g.FixAction("roll");
		}
		public static void MakeRollFrom(Game g, int r1, int r2)
		{
			//make step
			g.LastRoll = new[] { r1, r2 };
			g.Curr.LastRoll = g.LastRoll;
			g.FixAction("roll");
		}
		public static int[] ManualRolls(Game g)
		{
			var p = g.Curr;
			var pls = g.Players.Where(x => x.Id != p.Id);

			int rr2 = 0;

			if (pls.Any())
				rr2 = (int)Math.Round(pls.Average(x => x.ManRoll == 0 ? RNumber : x.ManRoll));

			var ll = g.LastRoll != null ? g.LastRoll[0] : p.Pos;

			int rr1 = p.IsBot ? (RNumber + ll) % 6 + 1 : p.ManRoll;

			return new[] { rr1, rr2 };
			//return new[] { 3, 2 };

		}

		public static int RNumber
		{
			get
			{
				var r = new Random(DateTime.Now.Millisecond);
				return r.Next(1, 6);
			}

		}

		public static int[] RandomRolls()
		{

			var r = new Random(DateTime.Now.Millisecond);
			var r1 = r.Next(1, 6);
			var r2 = r.Next(1, 6);
			return new[] { r1, r2 };

		}

		#endregion

		public static void LifeTimerJob(Object s)
		{
			lock (s)
			{
				var g = s as Game;
				//check if finish
				if (g.IsFinished)
				{
					g.StopTimer();
					return;
				}

				try
				{
					CheckState(g);
				}
				catch (Exception ex)
				{
					g.Logs.Add(ex.ToString());
				}

				if (g.CurrTimeOut < 0)
				{
					//if (State == GameState.Auction) BrainBot.CheckAuction(this, Curr);
					//LogicBot.MakeStep(this, Curr.Name, 3);
					//FinishAction();
				}

			}
		}

		public static void CheckState(Game g)
		{
			switch (g.State)
			{
				case GameState.BeginStep:
					RunBeginStepJob(g);
					break;

				case GameState.Auction:
					RunAuctionJob(g);
					break;

				case GameState.Trade:
					RunTradeJob(g);
					break;

				case GameState.CantPay:
				case GameState.NeedPay:
					CheckPayState(g);
					break;
			}

			if (g.State == GameState.EndStep)
			{
				if (g.Curr.IsBot) BotBrain.BotActionWhenFinishStep(g);
				g.FinishRound();
			}

		}

		#region Game Jobs

		public static void RunBeginStepJob(Game g)
		{
			try
			{
				if (g.IsManualMode)
				{
					var allPlayersRoll = g.Players.Where(x => !x.IsBot).All(x => x.ManRoll != 0);

					if (allPlayersRoll)
					{
						PlayerStep.MakeStep(g);
					}
				}
				else
				{
					if (g.Curr.IsBot)
						PlayerStep.MakeStep(g);
				}


			}
			catch (Exception ex)
			{
				g.Tlog("GameMan.CheckState.rror", "exception {0}", "exception {0}", ex.StackTrace);
				throw;
			}
		}

		public static void RunAuctionJob(Game g)
		{
			var pl = GetNextAuctionPlayer(g);
			if (pl != null && pl.InAuction && pl.IsBot)
			{
				//g.Tlog("CheckAuctionJob", "BotBrain.CheckAuction nextId  {0}", "BotBrain.CheckAuction nextId  {0}", nextId);
				GameManager.CheckAuction(g, pl);

			}
			GameManager.CheckAuctionWinner(g);
		}

		public static void RunTradeJob(Game g)
		{
			var trade = g.CurrTrade;
			if (trade != null)
			{
				if (trade.from.IsBot && trade.to.IsBot)
				{
					GameManager.MakeTrade(g);
					if (g.conf.DebugMode) g.Tlog("GameMan.CheckTradeJob.Bot2Bot", "[DebugMode][MakeTrade] bot->bot ", "[DebugMode][MakeTrade] bot->bot ");
				}

				if (!trade.from.IsBot && trade.to.IsBot)
				{
					var res = BotBrainTrade.MakeTradeFromPlayer(g);
					if (!res)
					{
						g.Tlog("GameMan.CheckTradeJob.Human2Bot", "[{0} don't want trade]", "[{0} не хочет меняться]", trade.to.Id);
					}
					//if (g.DebugMode) g.Tlog("[DebugMode][MakeTradeFromPlayer] human->bot");
				}

				if (!trade.to.IsBot)
				{
					var res = CheckBotToHumanRejectedTrades(g);
					if (res)
					{
						g.ToBeginRound();
					}
				}

			}

		}

		private static Player GetNextAuctionPlayer(Game g)
		{

			int nextId = g.currAuction.LastBiddedPlayer != null ?
				g.currAuction.LastBiddedPlayer.Id :
				g.Curr.Id;

			var pls = g.Players.Where(x => x.InAuction).Select(x => x.Id).ToArray();

			Player next = null;

			for (int i = 0; i < pls.Length; i++)
			{
				if (pls[i] == nextId)
					if (i != pls.Length - 1)
						next = g.GetPlayer(pls[i + 1]);
					else next = g.GetPlayer(pls[0]);
			}

			g.currAuction.CurrPlayer = next;

			if (g.conf.DebugMode)
				g.Tlog("GetNextAuctionPlayer", "next {0}", "next {0}", next.Id);
			return next;
		}

		public static void CheckAuction(Game g, Player bot)
		{
			var p = bot;

			var cell = g.currAuction.cell;

			var sum = g.GetPlayerCash(p.Id);

			var fact = BotBrain.FactorOfBuy(g, p, cell);

			var aucSum = cell.Cost * fact;

			if (sum >= aucSum && aucSum > g.currAuction.nextBid)
			{
				g.currAuction.currBid = g.currAuction.nextBid;
				g.Tlog("Auction.Yes", "@p{0} дает {1}", "@p{0} bid {1}", p.Id, g.currAuction.currBid.PrintMoney());
				g.currAuction.LastBiddedPlayer = p;

			}
			else
			{
				g.Tlog("Auction.No", "@p{0} выбывает", "@p{0} is out", p.Id);
				//g.aucState.allPlayers = g.aucState.allPlayers.Where(x => x.Id != p.Id).ToList();
				p.InAuction = false;
			}

		}

		public static void CheckPayState(Game g)
		{
			if (g.Curr.IsBot)
			{
				var res = PlayerAction.Pay(g);
				if (!res)
					LeaveGame(g, g.Curr.Name);
			}
		}

		private static bool CheckBotToHumanRejectedTrades(Game g)
		{
			var tr = g.CurrTrade;
			foreach (var rtr in g.RejectedTrades)
			{
				if (rtr.Equals(tr)) return true;
			}
			return false;
		}

		public static void AddToRejectedTrades(Game g)
		{
			if (!g.RejectedTrades.Any(x => x.Equals(g.CurrTrade)))
				g.RejectedTrades.Add(g.CurrTrade);
		}

		#endregion


		public static void LeaveGame(Game g, string uname)
		{
			if (g.IsFinished) return;

			var p = g.GetPlayer(uname);

			if (p == null) return;

			if (g.Players.Count > 2)
			{
				foreach (var cell in g.Map.CellsByUser(p.Id))
				{
					cell.Owner = null;
					cell.HousesCount = 0;
				}
				g.Tlog("Game.LeaveGame.PlayerLeaveGame", "@p{0} покинул игру", "@p{0} leave Game ", p.Id);
				//g.Players.Remove(p);
				p.Deleted = true;
				g.pcount--;
				g.OnLeave(p.Name, false);


			}
			else
				if (g.Players.Count == 2)
				{
					g.Winner = g.Players.First(x => x.Id != p.Id);
					g.Tlog("Game.LeaveGame.PlayerIsWinner", "@p{0} ПОБЕДИТЕЛЬ", "@p{0} is WINNER ", g.Winner.Id);
					g.SetState(GameState.FinishGame);
					g.OnLeave(p.Name, false);
					g.OnLeave(g.Winner.Name, true);

				}
		}

		#region Trade

		public static void InitTrade(Game g, string state, string userName)
		{
			Player from = g.GetPlayer(userName);
			InitTrade(g, state, from.Id);
		}

		public static void InitTrade(Game g, string state, int pid)
		{
			state = state.Replace("undefined", "");
			var pst = state.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

			Player from = g.GetPlayer(pid);

			var ss_from = pst.SingleOrDefault(x => x.StartsWith("p" + from.Id));

			var ss_to = pst.Where(x => !x.StartsWith("p" + from.Id)).OrderByDescending(x => x.Length).FirstOrDefault();
			if (ss_to == null)
			{
				return;
			}


			Player to = null;
			var q = ss_from.Length > 1;

			if (q)
			{
				foreach (var pp in g.Players)
				{
					if (ss_to.StartsWith("p" + pp.Id)) to = pp;
				}

				var from_ids = ss_from.Replace("p" + from.Id, "").Trim('-').Split('-').Where(x => !x.StartsWith("m")).Select(x => Int32.Parse(x)).ToArray();
				var from_money_str = ss_from.Trim('-').Split('-').FirstOrDefault(x => x.StartsWith("m"));
				var from_money = from_money_str != null ? from_money_str.Replace("m", "") : "";

				var to_ids = ss_to.Replace("p" + to.Id, "").Trim('-').Split('-').Where(x => !x.StartsWith("m")).Select(x => Int32.Parse(x)).ToArray();
				var to_money_str = ss_to.Trim('-').Split('-').FirstOrDefault(x => x.StartsWith("m"));
				var to_money = to_money_str != null ? to_money_str.Replace("m", "") : "";

				var trState = new Trade
				{
					from = from,
					give_cells = from_ids,
					giveMoney = string.IsNullOrEmpty(from_money) ? 0 : Int32.Parse(from_money),
					to = to,
					get_cells = to_ids,
					getMoney = string.IsNullOrEmpty(to_money) ? 0 : Int32.Parse(to_money),
				};
				g.CurrTrade = trState;

			}
		}

		public static void MakeTrade(Game g)
		{
			g.CompletedTrades.Add(g.CurrTrade);

			Player firstPlayer = g.CurrTrade.from;
			Player secondPlayer = g.CurrTrade.to;


			var first_give = g.CurrTrade.give_cells.Select(x => g.Cells[x]).ToList();
			var second_give = g.CurrTrade.get_cells.Select(x => g.Cells[x]).ToList();

			first_give.ForEach(x => x.Owner = secondPlayer.Id);
			second_give.ForEach(x => x.Owner = firstPlayer.Id);
			//money 
			firstPlayer.Money += g.CurrTrade.getMoney * 1000;
			firstPlayer.Money -= g.CurrTrade.giveMoney * 1000;

			secondPlayer.Money += g.CurrTrade.giveMoney * 1000;
			secondPlayer.Money -= g.CurrTrade.getMoney * 1000;

			g.Map.UpdateMap();
			g.Tlog("Trade.TradeOk", "обмен соостоялся", "Trade completed");
			g.FixAction(string.Format("tradeOk_give[{0}]_get[{1}]",
				string.Join(",", g.CurrTrade.give_cells),
				string.Join(",", g.CurrTrade.get_cells)));

			g.ToBeginRound();
		}

		#endregion

		#region Auction

		public static void InitAuction(Game g)
		{
			var cell = g.CurrCell;

			g.Tlog("GameMan.InitAuction", "[Аукцион] земля={0}", "[Auction] cell={0}", cell.Name);

			g.currAuction = new AuctionState();

			g.currAuction.cell = cell;
			g.currAuction.currBid = cell.Cost;

			g.currAuction.CurrPlayer = g.Curr;

			g.Players.ForEach(x => x.InAuction = true);

			//if (g.aucState.allPlayers.Count() < 1)
			//{
			//    g.Tlog("step.Auction.NoPlayers", "АУКЦИОН нет игроков с деньгами для аукциона", "AUCTION no players with money");
			//    g.Finish();
			//}
			//else if (g.aucState.allPlayers.Count() == 1)
			//{
			//    g.Tlog("step.Auction.OnePlayer", "АУКЦИОН вы можете купить", "AUCTION you can buy");
			//}
		}

		public static void CheckAuctionWinner(Game g)
		{
			var count = g.Players.Count(x => x.InAuction);

			if (count == 0 || (g.currAuction.LastBiddedPlayer != null && count == 1))
				g.currAuction.IsFinished = true;

			if (g.currAuction.IsFinished)
			{
				var last = g.currAuction.LastBiddedPlayer;
				if (last != null)
				{
					if (BotBrain.MortgageSell(g, last, g.currAuction.currBid))
					{
						g.Tlog("Game.CheckAuctionWinner.Winner", "@p{0} выиграл", "@p{0} winner", last.Id);
						//set winner
						GameManager.SetAuctionWinner(g);
					}
				}
				//g.Tlog("Game.CheckAuctionWinner.FinishStepForBot", "FinishStepForBot", "FinishStepForBot");

				g.FinishStep();

			}
		}

		public static void SetAuctionWinner(Game g)
		{
			var cell = g.currAuction.cell;
			var p = g.currAuction.LastBiddedPlayer;

			cell.Owner = p.Id;
			p.Money -= g.currAuction.currBid;
			g.Map.UpdateMap();

		}

		#endregion

	}
}
