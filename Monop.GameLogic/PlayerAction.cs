using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
	public class PlayerAction
	{


		#region Pay

		public static void PayToPolice(Game g)
		{
			var p = g.Curr;

			var cell = g.Cells[p.Pos];

			if (p.CaughtByPolice)
			{
				g.Tlogp("PlayerAction.Pay.PayPolice", "заплатил за выход", "you paid for exit");
				if (p.Police == 4)
				{
					if (Pay(g, 500000))
					{
						p.Police = 0;
						g.Curr.Step();
						PlayerStep.ProcessPosition(g);
					}
				}
				else
				{
					p.Police = 0;
					Pay(g, 500000);
					g.ToBeginRound();
				}

			}
		}

		public static bool Pay(Game g, int amount)
		{
			g.PayAmount = amount;
			return Pay(g);
		}

		public static bool Pay(Game g, bool needFinish = true)
		{
			//if (g.State != GameState.NeedPay) return ;

			var curr = g.Curr;
			var amount = g.PayAmount;
			//if (amount == 0) return true;

			var ok = curr.IsBot ?
				BotBrain.MortgageSell(g, curr, amount)
				: curr.Money > amount;

			if (ok)
			{
				curr.Money -= amount;
				if (g.PayToUserId.HasValue)
				{
					var to = g.GetPlayer(g.PayToUserId.Value);
					to.Money += amount;
					g.PayToUserId = null;
				}
				if (needFinish) g.FinishStep("paid_" + g.PayAmount.PrintMoney());
				else g.SetState(GameState.BeginStep);

				g.PayAmount = 0;
				return true;
			}
			else
			{
				g.Tlogp("PlayerAction.PayAmount", "не хватает денег", "not enough money");
				g.ToCantPay();
			}
			return false;
		}

		#endregion


		public static void Buy(Game g)
		{
			if (g.State != GameState.CanBuy) return;

			var p = g.Curr;

			var cell = g.CurrCell;

			if (cell.IsLand)
			{
				if (cell.Owner == null)
				{
					//--buy
					var ff = BotBrain.FactorOfBuy(g, p, cell);

					bool needBuy = ff >= 1;

					if (p.IsBot)
					{
						if (ff >= 1 && p.Money < cell.Cost)
							needBuy = BotBrain.MortgageSell(g, cell.Cost);
					}
					else
					{
						if (p.Money < cell.Cost)
						{
							g.ToCantPay();
							return;
						}
					}

					if (needBuy)
					{
						g.Map.SetOwner(p, cell);
						g.Tlogp("PlayerAction.Bought", "вы купили {0} за {1}", "You bought {0} for {1}", cell.Name, cell.Cost.PrintMoney());
						g.FinishStep(string.Format("bought_{0}_f({1})", cell.Id, ff));
					}
					else
					{
						g.ToAuction();
					}
				}
			}

		}

		public static bool MakeTrade(Game g, string userName, bool isYes)
		{
			if (g.CurrTrade.to.Id == g.GetPlayer(userName).Id && isYes)
			{
				GameManager.MakeTrade(g);
				return true;
			}
			else
			{
				GameManager.AddToRejectedTrades(g);
				g.Tlogp("PlayerAction.Trade.AddToRej", "к игнорируемым {0}", "added to rejected trades {0}", g.CurrTrade.ToString());
				g.ToBeginRound();
				return false;
			}
		}

		public static void MakeAuctionYes(Game g, string userName)
		{
			var pl = g.GetPlayer(userName);

			if (pl == null) return;



			//StartAction(g);
			g.CleanTimeOut();

			var mm = g.GetPlayerCash(pl.Id);

			var LastBiddedPlayer = g.currAuction.LastBiddedPlayer;

			if (LastBiddedPlayer != null)
			{
				if (LastBiddedPlayer.Id == pl.Id) return;
			}

			if (mm >= g.currAuction.nextBid)
			{
				g.currAuction.currBid += 50000;
				g.Tlog("Auction.Yes", "@p{0} дает {1}", "@p{0} bid {1}", pl.Id, g.currAuction.currBid.PrintMoney());
				g.currAuction.LastBiddedPlayer = pl;

			}
			else
			{
				g.Tlog("PlayerAction.AuctionYes.NoMoney", "@p{0} не хватает денег ", "@p{0} no money", pl.Id);
				pl.InAuction = false;
			}
			//GameManager.CheckAuctionWinner(g);

		}

		public static void MakeAuctionNo(Game g, string userName)
		{
			var p = g.GetPlayer(userName);
			g.Tlog("PlayerAction.AuctionNo.PlayerOut", "@p{0} выбывает", "@p{0} is out", p.Id);
			p.InAuction = false;
			GameManager.CheckAuctionWinner(g);
		}


		public static bool Build(Game g, int pid, IEnumerable<int> ids)
		{
			var p = g.GetPlayer(pid);
			bool res = false;
			foreach (var id in ids)
			{
				var cell = g.Cells[id];

				if (cell.IsMonopoly)
				{
					if (p.Money >= cell.HouseCost)
					{
						if (cell.HousesCount < 5)
						{
							cell.HousesCount++;
							p.Money -= cell.HouseCost;
							res = true;
						}
					}
				}
			}
			return res;
		}

		public static void MortgageLands(Game g, int pid, IEnumerable<int> from_ids)
		{
			var p = g.GetPlayer(pid);

			foreach (var id in from_ids)
			{
				var c = g.Cells[id];
				if (!c.IsMortgage)
				{
					c.IsMortgage = true;
					p.Money += c.MortgageAmount;
				}
				else
				{
					if (c.IsMortgage)
					{
						if (p.Money >= c.UnMortgageAmount)
						{
							c.IsMortgage = false;
							p.Money -= c.UnMortgageAmount;
						}
					}
				}

			}
		}

		public static bool SellHouses(Game g, int pid, IEnumerable<int> ids)
		{
			var p = g.GetPlayer(pid);
			g.Tlog("PlayerAction.SellHouses.Sell", "@p{0} продал дом", "@p{0} sell house", p.Id);

			bool res = false;
			foreach (var id in ids)
			{
				var cell = g.Cells[id];

				if (cell.IsMonopoly)
				{

					if (cell.HousesCount > 0)
					{
						cell.HousesCount--;
						p.Money += cell.HouseCost;
						res = true;

					}

				}

			}

			return res;
		}
	}
}
