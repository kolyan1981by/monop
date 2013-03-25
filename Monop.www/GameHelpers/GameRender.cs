using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Text;
using System.IO;
using GameLogic;
using System.Xml.Linq;



namespace Monop.www.Helpers
{
    public class GameRender
    {

        public static string ShowGameState(Game g, string uname)
        {
            if (g == null) return "";

            var p = g.GetPlayer(uname);

            if (p == null) return g.Text("PlayerState.WatchingGame", "Вы наблюдаете за игрой", "You are watching game");

            string paction = "";

            string rolled = "";// string.Format("roll {0} <br />", g.Curr.PrintLastRoll);

            var IsCurrPlayer = g.IsCurrPlayer(uname);


            if (!IsCurrPlayer && !g.IsManualMode) paction = rolled + g.Text("Map.Waiting", "ждите ход...", "waiting...");

            if (p.IsBot) return rolled + g.Text("Map.RenderBotPlaying", "играет компьютер...", "Bot playing...");

            switch (g.State)
            {
                case GameState.BeginStep:
                    paction = GameRender.RenderStartState(g, uname);

                    paction += "<br />" + GameRender.RenderRollButton(g, uname);
                    break;

                case GameState.Mortgage:
                    paction = g.Text("Mortage.Cell", "земля заложена <br />", "land is mortage <br />");
                    break;

                case GameState.CanBuy:
                    if (IsCurrPlayer)
                    {
                        var tt = g.Text("pAction.CanBuy", "вы можете купить", "you can buy");
                        paction = string.Format("{0} {1}<br /> {2}", tt, g.CurrCell.Name,
                            UIParts("BuyOrAuc"));
                    }
                    break;

                case GameState.Auction:
                    paction = RenderAuction(g, uname);
                    break;

                case GameState.Trade:
                    paction = GameRender.RenderTradeProposal(g, uname);
                    break;

                case GameState.Pay:
                    if (IsCurrPlayer)
                    {
                        if (g.Curr.Police == 4)
                            paction = string.Format("{2} {0} {1} k$",
                                UIParts("ButtonPay"),
                                g.AmountOfPay / 1000,
                                g.Text("pAction.MustPay", "вы должны заплатить", "you must pay"));
                        else
                            paction = string.Format("{0} {1} k$", UIParts("ButtonPay"), g.AmountOfPay / 1000);
                    }
                    break;

                case GameState.CantPay:
                    if (IsCurrPlayer)
                    {
                        paction = g.Text("CannotPay",
                            "у вас нет денег, попробуйте заложить <br />",
                            "not enough money, try mortage land <br />");
                        paction += "<br />" + UIParts("ButtonPay");
                    }
                    //g.State = GameState.Pay;
                    break;


                case GameState.MoveToCell:
                    if (IsCurrPlayer)
                    {
                        if (g.LastRandomCard != null && g.LastRandomCard.Pos != 10)
                            paction = string.Format("{0} {1} {2}",
                                 UIParts("ButtonRandomMove"),
                                g.Text("go.RanodmMove", " на клетку ", " to cell "),
                                g.LastRandomCard.Pos);
                        else
                            paction = string.Format("{0} {1}", UIParts("ButtonRandomMove"),
                       g.Text("go.RanodmMove.Police", "вас задержал интерпол", "go to Police"));

                    }
                    break;

                case GameState.RandomCell:
                    if (g.IsCurrPlayer(uname))
                    {
                        paction = string.Format("{0} {1}",
                            g.LastRandomCard.Text,
                             UIParts("ButtonOK"));
                    }
                    else paction = g.LastRandomCard.Text;

                    break;

                default:
                    paction = "Nothing";
                    break;
            }

            return string.Format("{0}", paction);

            //var lr = g.LastRoll != null ? g.LastRoll : new[] { 1, 1 };
            //var roll = string.Format("{0}<br /> {1} {2}",
            //    g.Text("Map.LastRoll", "последний бросок", "last roll"),
            //    //g.Curr.htmlName,
            //    ImageHelper.GetRollImage(lr[0]),
            //    ImageHelper.GetRollImage(lr[1]));

            //return string.Format("{0} <br /> {1}", roll, paction);
        }

        private static string RenderStartState(Game g, string uname)
        {
            var p = g.GetPlayer(uname);

            if (p != null && p.CaughtByPolice && p.Id == g.Curr.Id)
            {
                if (p.Police == 4)
                    return string.Format("you must pay {0}", UIParts("ButtonPay"));
                else
                {

                    if (p.FreePoliceKey > 0)
                    {
                        var tt = g.Text("Map.OutPoliceWithKey", "Хотите заплатить за выход {0} или ключ {1} ", "want pay to out from POLICE {0} or use key {1}");
                        return string.Format(tt, UIParts("ButtonPay"), UIParts("ButtonPoliceUseKey"));
                    }
                    else
                    {
                        var tt = g.Text("Map.OutPolice", "Хотите заплатить за выход {0}", "want pay to out from POLICE {0}");
                        return string.Format(tt, UIParts("ButtonPay"));
                    }

                }
            }
            else
            {
                var tt = g.Text("Map.StepOf", "ходит...", "step of...");
                return string.Format("{0} {1}", tt, g.Curr.htmlName);
            }

        }

        public static string RenderRollButton(Game g, string uname)
        {
            var pl = g.GetPlayer(uname);

            if (g.State == GameState.BeginStep && pl != null)
                if (g.IsManualMode)
                {
                    if (pl.ManRoll != 0)
                    {
                        var tt = g.Text("manRoll.pressed", "you pressed ", "вы нажали ") + pl.ManRoll;
                        return tt;
                    }
                    return MVCHelper.RenderPartialToString("~/Controls/Command.ascx", g);
                }
                else
                    if (pl.Id == g.Curr.Id && !pl.IsBot)
                        return UIParts("ButtonRoll");
            return "wait...";
        }


        public static string RenderTradeProposal(Game g, string userName)
        {
            var res = "player {0} want trade with {1} <br />player {0} -> {2} <br /> player {1} -> {3} <br />";

            var action = "<input id=\"btnTrY\" type=\"button\" value=\"yes\" onclick=\"action('tr_y');\" />" +
            "<input id=\"btnTrNo\" type=\"button\" value=\"no\" onclick=\"action('tr_no');\" />";

            //request of player
            var pl = g.GetPlayer(userName);

            var fr = g.CurrTrade.from.htmlName;
            var to = g.CurrTrade.to.htmlName;

            var fromProposal = g.CurrTrade.give_cells.Aggregate("", (acc, i) => acc + "," + g.Cells[i].Name);
            fromProposal += g.CurrTrade.giveMoney == 0 ? "" : string.Format(", money = {0}K", g.CurrTrade.giveMoney);

            var toProposal = g.CurrTrade.get_cells.Aggregate("", (acc, i) => acc + "," + g.Cells[i].Name);
            toProposal += g.CurrTrade.getMoney == 0 ? "" : string.Format(", money = {0}K", g.CurrTrade.getMoney);

            if (pl != null && pl.Id == g.CurrTrade.to.Id)
            {
                return string.Format(res + action, fr, to, fromProposal, toProposal);
            }
            else return string.Format(res, fr, to, fromProposal, toProposal);

        }

        public static string RenderAuction(Game g, string userName)
        {
            if (g == null) return "finish game";

            //var res = MVCHelper.RenderPartialToString("Game/_auction", null);
            var res = UIParts("HtmlAuction");

            if (g.currAuction == null) return "auction is null";

            var curr = g.currAuction.CurrPlayer;
            if (curr == null) return "wait...";
            if (curr.Name != userName) return "wait...";

            if (g.currAuction.IsFinished)
            {
                var pl = g.currAuction.LastBiddedPlayer;
                if (pl != null)
                    return string.Format("{0} is winner, your bid is {1}", pl.htmlName, g.currAuction.currBid);
                else
                    return string.Format(" bid is {0}, no players", g.currAuction.currBid);

            }
            else
            {
                var startBid = g.currAuction.cell.Cost;
                var bid = g.currAuction.currBid;

                var pls = g.Players.Where(x => x.InAuction).Select(x => x.Name).ToArray();

                return string.Format(res, startBid, bid, bid + 50000, g.currAuction.cell.Name, string.Join(":", pls));

            }
            return res;
        }


        public static string UIParts(string key)
        {

            var dd = System.Web.HttpContext.Current.Application["UIParts"] as Dictionary<string, string[]>;
            if (dd == null)
                dd = LoadUIXml(System.Web.Hosting.HostingEnvironment.MapPath(ConfigHelper.ResDir));

            return dd[key.ToLower()][1];
        }

        static Dictionary<string, string[]> LoadUIXml(string path)
        {

            var ui_xml = File.ReadAllText(Path.Combine(path, ConfigHelper.UIPartsFile));

            var xDocument = XDocument.Parse(ui_xml);

            var res = xDocument.Descendants("text")
                .ToDictionary(x => x.Attribute("key").Value.ToLower(),
                x => new string[]
			{
				x.Element("ru").Value,
				x.Element("en").Value
			});

            return res;

        }
    }
}
