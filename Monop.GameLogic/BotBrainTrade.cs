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

        //example i give france-1 (he has 2), i get russia-1 (i has 2)
        private static Trade CheckOnPlayersCells(Game g, TradeRule ex, int my)
        {

            //you give
            var getCells = g.Map.CellsByGroup(ex.GetLand)
                .Where(x => x.Owner.HasValue && x.Owner != my && x.OwGrCount == ex.GetCount);

            if (!getCells.Any()) return null;

            var pfrom = g.GetPlayer(my);

            var pto = g.GetPlayer(getCells.First().Owner.Value);

            // i have 
            var _myCells = g.Map.CellsByUserByGroup(my, ex.GetLand).Count() == ex.MyCount;

            //you have
            var _yourCells = g.Map.CellsByUserByGroup(pto.Id, ex.GiveLand).Count() == ex.YourCount;

            //i give to you 
            var giveCells = g.Map.CellsByUserByGroup(my, ex.GiveLand);

            //money factor
            var money1 = g.GetPlayerAssets(my, false);
            var money2 = g.GetPlayerAssets(pto.Id, false);

            var mfac = (money1 / (double)money2) >= ex.MoneyFactor;

            if (giveCells.Count() == ex.GiveCount && _myCells && _yourCells && mfac)
            {
                //g.trState = new TradeState
                return new Trade
                {
                    from = g.GetPlayer(my),
                    give_cells = giveCells.Select(x => x.Id).ToArray(),
                    giveMoney = ex.GiveMoney,
                    //fromMoney = string.IsNullOrEmpty(from_money) ? 0 : Int32.Parse(from_money),
                    to = pto,
                    get_cells = getCells.Select(x => x.Id).ToArray(),
                    getMoney = ex.GetMoney,
                    ExchId = ex.Id,
                };
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
