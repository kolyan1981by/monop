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
    public class GameAction
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

            if (g.State == GameState.CanBuy)
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
                    PlayerAction.AuctionYes(g, userName);
                }
                else if (act == "auc_no")
                {
                    PlayerAction.AuctionNo(g, userName);
                }
                //LogicBot.CheckAuctionJob(g);
            }

            if (g.State == GameState.Pay)
                PlayerAction.StateOfPay(g);

            if (g.State == GameState.CantPay)
            {
                res = g.Text("go.CannotPay", "у вас нет денег, попробуйте заложить или продать <br />",
                    "you dont have money,try mortage <br />");
                   
                res += "<br />" + GameRender.UIParts("ButtonPay");
                //PlayerAction.Pay(g);
            }

            if (g.State == GameState.Trade)
            {
                PlayerAction.Trade(g, userName, act == "tr_y");
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
        public static void InitTrade(Game g, string state, string userName)
        {
            g.SetState(GameState.Trade);
            state = state.Replace("undefined", "");
            var pst = state.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            Player from = g.GetPlayer(userName);

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
                var from_money = ss_from.Trim('-').Split('-').FirstOrDefault(x => x.StartsWith("m")).Replace("m", "");

                var to_ids = ss_to.Replace("p" + to.Id, "").Trim('-').Split('-').Where(x => !x.StartsWith("m")).Select(x => Int32.Parse(x)).ToArray();
                var to_money = ss_to.Trim('-').Split('-').FirstOrDefault(x => x.StartsWith("m")).Replace("m", "");

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

        public static bool BuildOrSell(Game g, string state, string action, string userName)
        {
            //dont build if user on your land

            if (action == "build" && (g.State == GameState.Pay || g.State == GameState.CantPay)) return false;

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
