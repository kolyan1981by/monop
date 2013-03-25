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

        public static int[] ManuallRolls(int r1)
        {

            var r = new Random(DateTime.Now.Millisecond);
            var r2 = DateTime.Now.Second % 6 + 1;
            return new[] { r1, 3 };
            //return new[] { 3, 2 };

        }

        public static int[] ManualRolls(Game g)
        {
            var p = g.Curr;
            var pls = g.Players.Where(x => x.Id != p.Id);

            int rr2 = 0;

            if (pls.Any())
                rr2 = (int)Math.Round(pls.Average(x => x.ManRoll == 0 ? RNumber : x.ManRoll));

            //var ll = g.LastRoll != null ? g.LastRoll[1] : p.Pos;
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
            if (g.State == GameState.BeginStep)
            {
                try
                {
                    if (!g.IsManualMode)
                    {
                        if (g.Curr.IsBot)
                            PlayerStep.MakeStep(g);
                    }
                    else
                    {
                        var allPlayersRoll = g.Players.Where(x => !x.IsBot).All(x => x.ManRoll != 0);

                        if (allPlayersRoll)
                        {
                            PlayerStep.MakeStep(g);
                        }
                    }

                }
                catch (Exception ex)
                {
                    g.Tlog("GameMan.CheckState.rror", "exception {0}", "exception {0}", ex.StackTrace);
                    throw;
                }
            }

            if (g.State == GameState.Auction)
            {
                GameManager.CheckAuctionJob(g);
            }

            if (g.State == GameState.Trade)
            {
                GameManager.CheckTradeJob(g);
            }
            if (g.State == GameState.CantPay || g.State == GameState.Pay)
            {
                if (g.Curr.IsBot)
                {
                    PlayerAction.Pay(g, g.AmountOfPay);
                    //g.FinishRoundForBot();
                }
            }

            if (g.State == GameState.EndStep)
            {
                if (g.Curr.IsBot)
                    BotBrain.BotActionWhenFinishStep(g);

                g.FinishRound();
            }
        }

        #region Game Jobs

        public static void CheckAuctionJob(Game g)
        {
            var pl = GetNextAuctionPlayer(g);
            if (pl != null && pl.InAuction && pl.IsBot)
            {
                //g.Tlog("CheckAuctionJob", "BotBrain.CheckAuction nextId  {0}", "BotBrain.CheckAuction nextId  {0}", nextId);
                GameManager.RunAuction(g, pl);

            }
            GameManager.CheckAuctionWinner(g);
        }

        public static void CheckTradeJob(Game g)
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

        public static void RunAuction(Game g, Player bot)
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

            if (g.pcount > 2)
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
                if (g.pcount == 2)
                {
                    g.Winner = g.Players.First(x => x.Id != p.Id);
                    g.Tlog("Game.LeaveGame.PlayerIsWinner", "@p{0} ПОБЕДИТЕЛЬ", "@p{0} is WINNER ", g.Winner.Id);
                    g.SetState(GameState.FinishGame);
                    g.OnLeave(p.Name, false);
                    g.OnLeave(g.Winner.Name, true);

                }
        }

        public static void MakeTrade(Game g)
        {
            g.CompletedTrades.Add(g.CurrTrade);

            Player pfrom = g.CurrTrade.from;
            Player pto = g.CurrTrade.to;


            var fromCells = g.CurrTrade.give_cells.Select(x => g.Cells[x]).ToList();
            var toCells = g.CurrTrade.get_cells.Select(x => g.Cells[x]).ToList();

            fromCells.ForEach(x => x.Owner = pto.Id);
            toCells.ForEach(x => x.Owner = pfrom.Id);
            //money 
            pfrom.Money += g.CurrTrade.getMoney * 1000;
            pfrom.Money -= g.CurrTrade.giveMoney * 1000;

            pto.Money += g.CurrTrade.giveMoney * 1000;
            pto.Money -= g.CurrTrade.getMoney * 1000;

            g.Map.UpdateMap();
            g.Tlog("Trade.TradeOk", "обмен соостоялся", "Trade completed");
            g.ToBeginRound();
        }

        public static void InitAuction(Game g)
        {
            var cell = g.CurrCell;

            g.Tlog("GameMan.InitAuction", "[Аукцион] земля={0}", "[Auction] cell={0}", cell.Name);

            g.SetState(GameState.Auction);
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


    }
}
