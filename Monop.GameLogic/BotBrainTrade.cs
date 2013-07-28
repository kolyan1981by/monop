using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
	public class BotBrainTrade
	{

		public static bool TryDoTrade(Game g)
		{
			//var res = MakeExchange(g, g.Curr);

			var trs = GetValidTrades(g, g.Curr);

			Trade tr = null;

			foreach (var item in trs)
			{
				var res = g.RejectedTrades.Any(x => x.Equals(item));
				if (!res)
				{
					tr = item;
					break;
				}
			}

			if (tr != null)
			{
				g.CurrTrade = tr;
				g.Tlog("CheckTradeJob.FromBotToHuman", "Обмен между {0} и {1}", "Exchange between {0} and {1}", tr.from.Name, tr.to.Name);
				g.SetState(GameState.Trade);
				//GameManager.CheckTradeJob(g);
				return true;
			}

			return false;

		}

		private static IEnumerable<Trade> GetValidTrades(Game g, Player p)
		{
			foreach (var rule in g.GetTradeRules(p.Id))
			{
				var tr = CheckOnPlayersCells(g, rule, p.Id);

				if (tr == null && !p.OneDirection)
				{
					tr = CheckOnPlayersCells(g, ReverseRule(rule), p.Id);
					if (tr != null) tr.Reversed = true;
				}

				if (tr != null)
				{
					yield return tr;
				}
			}
		}

		//example
		//from player: from have 2_russia + another 1_russia
		//to: to have 2_france + 1_france
		private static Trade CheckOnPlayersCells(Game g, TradeRule trd, int my)
		{
			var pfrom = g.GetPlayer(my);

			//from_pl get cells
			var wantedCellsGroupedByUser = g.Map.CellsByGroup(trd.GetLand)
				.Where(x => x.Owner.HasValue && x.Owner != my)
				.GroupBy(x => x.Owner.Value);

			if (!wantedCellsGroupedByUser.Any()) return null;

			//process for each player
			foreach (var wantedByUser in wantedCellsGroupedByUser)
			{
				if (wantedByUser.Count() != trd.GetCount) continue;


				var pto = g.GetPlayer(wantedByUser.First().Owner.Value);

				// i have 
				var _myCells = g.Map.CellsByUserByGroup(my, trd.GetLand).Count() == trd.MyCount;

				//you have
				var _yourCells = g.Map.CellsByUserByGroup(pto.Id, trd.GiveLand).Count() == trd.YourCount;

				//i give to you 
				var giveCells = g.Map.CellsByUserByGroup(my, trd.GiveLand);

				//money factor
				var money1 = g.GetPlayerAssets(my, false);
				var money2 = g.GetPlayerAssets(pto.Id, false);

				var mfac = (money1 / (double)money2) >= trd.MoneyFactor;

				if (giveCells.Count() == trd.GiveCount && _myCells && _yourCells && mfac)
				{
					//g.trState = new TradeState
					return new Trade
					{
						from = g.GetPlayer(my),
						give_cells = giveCells.Select(x => x.Id).ToArray(),
						giveMoney = trd.GiveMoney,
						//fromMoney = string.IsNullOrEmpty(from_money) ? 0 : Int32.Parse(from_money),
						to = pto,
						get_cells = wantedByUser.Select(x => x.Id).ToArray(),
						getMoney = trd.GetMoney,
						ExchId = trd.Id,
					};
				}
			}
			return null;
		}

		private static TradeRule ReverseRule(TradeRule ex)
		{
			var reversed = new TradeRule();

			reversed.Id = ex.Id;

			reversed.MyCount = ex.YourCount;
			reversed.YourCount = ex.MyCount;

			reversed.GetCount = ex.GiveCount;
			reversed.GetLand = ex.GiveLand;
			reversed.GetMoney = ex.GiveMoney;

			reversed.GiveCount = ex.GetCount;
			reversed.GiveLand = ex.GetLand;
			reversed.GiveMoney = ex.GetMoney;

			return reversed;

		}

		#region trade from player

		public static bool MakeTradeFromPlayer(Game g)
		{
			var ptrade = g.CurrTrade;
			if (ptrade.to.IsBot)
			{
				var trs = GetValidTrades(g, g.CurrTrade.to);

				if (IsGoodTrade(ptrade, trs))
				{
					GameManager.MakeTrade(g);
					g.FixAction("trade_completed");
					return true;
				}
			}
			g.ToBeginRound();
			return false;
		}

		private static bool IsGoodTrade(Trade ptr, IEnumerable<Trade> trs)
		{
			foreach (var tr in trs)
			{
				if (tr.to.Id == ptr.from.Id)
				{
					if (EqualTrCells(tr.get_cells, ptr.give_cells)
						&& ptr.giveMoney >= tr.giveMoney && ptr.getMoney <= tr.getMoney)
						return true;
				}
			}
			return false;
		}

		private static bool EqualTrCells(int[] p1, int[] p2)
		{
			var a1 = p1.OrderBy(x => x).ToArray();
			var a2 = p2.OrderBy(x => x).ToArray();
			var ll = Math.Min(a1.Length, a2.Length);

			for (int i = 0; i < ll; i++)
			{
				if (a1[i] != a2[i]) return false;
			}
			return true;
		}


		#endregion


	}
}
