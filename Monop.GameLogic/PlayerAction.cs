using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    public class PlayerAction
    {


        #region Pay

        public static void StateOfPay(Game g)
        {
            var p = g.Curr;
            var cell = g.Cells[p.Pos];

            if (cell.IsLand)
            {
                var rent = cell.Rent;
                //power station
                if (cell.Group == 33)
                {
                    var rrs = p.LastRoll[0] + p.LastRoll[1];
                    rent = rrs * cell.Rent;
                }

                var to = g.GetPlayer(cell.Owner.Value);

                if (PayAndFinish(g, rent, to))
                    g.Tlogp("PlayerAction.Pay.PaidRent", "заплатил ренту {0}", "paid rent {0}", rent.PrintMoney());

            }

            if (cell.Type == 6)
            {
                if (PayAndFinish(g, cell.Rent))
                    g.Tlogp("PlayerAction.PayTax.PayTaxOk", "заплатил налог {0}", "you pay tax {0}", cell.Rent.PrintMoney());

            }

            if (cell.Type == 4)
            {
                var rMess = g.LastRandomCard;

                if (rMess.RandomGroup == 4)
                {
                    //g.logp(g.Text("PlayerAction.Pay.Rand4.PayEachPlayer", "заплатите каждому игроку 500K", "pay each player 500K"));

                    var allMoney = g.AmountOfPay;
                    if (PayAndFinish(g, allMoney))
                    {
                        g.Players.Where(x => x.Id != p.Id).ToList().ForEach(x => x.Money += rMess.Money);
                    }

                }
                if (rMess.RandomGroup == -1 || rMess.RandomGroup == 15)
                {
                    PayAndFinish(g, rMess.Money);
                }
            }

        }

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

        static bool PayAndFinish(Game g, int amount, Player to = null)
        {
            if (Pay(g, amount, to))
            {
                g.FinishStep("paid_" + amount);
                return true;
            }
            return false;
        }

        public static bool Pay(Game g, int amount, Player to = null)
        {
            var p = g.Curr;
            var ok = (p.IsBot ? BotBrain.MortgageSell(g, p, amount) : p.Money > amount);

            if (ok)
            {
                p.Money -= amount;
                if (to != null) to.Money += amount;

                return true;
            }
            else
            {
                if (p.IsBot)
                {
                    g.Tlogp("LeaveGame", "вы банкрот и покидаете игру", "you are bankrupt");
                    GameManager.LeaveGame(g, p.Name);
                }
                else
                {
                    g.Tlogp("PlayerAction.PayAmount", "не хватает денег", "not enough money");
                    g.ToCantPay();
                }
            }
            return false;
        }

        #endregion

        public static void Buy(Game g)
        {
            var p = g.Curr;

            var cell = g.CurrCell;

            if (cell.IsLand)
            {
                if (cell.Owner == null)
                {
                    //--buy
                    var ff = BotBrain.FactorOfBuy(g, p, cell);

                    var needBuy = (p.IsBot ? ff >= 1 : p.Money >= cell.Cost);

                    if (needBuy)
                    {
                        g.Map.SetOwner(p, cell);
                        g.Tlogp("PlayerAction.Bought", "вы купили {0} за {1}", "You bought {0} for {1}", cell.Name, cell.Cost.PrintMoney());
                        g.FinishStep(string.Format("bought_{0}_f({1})", cell.Id, ff));
                    }
                    else
                    {
                        //-- auction
                        g.ToAuction();
                    }
                }
            }

        }

        public static bool Trade(Game g, string userName, bool isYes)
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

        public static void AuctionYes(Game g, string userName)
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

        public static void AuctionNo(Game g, string userName)
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
