using GameLogic;
using Monop.www.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Monop.www.GameHelpers
{
    public class SimHelper
    {
        public static string[] lands = new[] { "Spain", "Greece", "Italy", "England", "Russia", "France", "Usa", "Japan" };

        public static string GetPlayersInfo(GameLogic.GameAction ra)
        {
            string text = "";
            foreach (Player pl in ra.Players)
            {

                text += string.Format("{0}, money={1} worth={2}<br />",
                    pl.htmlName,
                    pl.Money.PrintMoney(),
                    HTML.PrintWithColor(GameHelper.GetPlayerAssets(pl.Id, pl.Money, ra.Cells, true))
                    );

            }
            return text;
        }

        public static List<RoundRec> ShowActions(Game g)
        {
            var res = new List<RoundRec>();
            foreach (var gr in g.RoundActions.GroupBy(x => x.round))
            {
                res.Add(RoundLog(gr.Key, gr.ToList()));
            }
            return res;
        }

        public static RoundRec RoundLog(int rid, List<GameLogic.GameAction> gr)
        {
            string text = "";
            text += string.Format("r{0}, {1} <br />", rid, HTML.PlName(gr.First().curr, "bot-", true));

            foreach (var act in gr)
            {
                if (act.action == "roll")
                {
                    text += string.Format("[p={0}: {1}={2}]", act.cpos, act.action, act.croll.PrintRoll());
                }
                else
                {
                    text += string.Format("[p={0}: {1}]", act.cpos, act.action);
                }

                if (act.action == "random")
                {
                    var card = act.RandomCard;

                    text += card.Text + (card.RandomGroup == 2 ? ":pos=" + card.Pos : "");
                }
                text += "<br />";

            }
            return new RoundRec { round = rid, text = text };
        }

        public static List<int[]> GetChartData(Game game)
        {
            var res = new List<int[]>();

            foreach (var gr in game.RoundActions.GroupBy(x => x.round))
            {
                var ra = gr.First();
                res.Add(ra.Players.Select(pl =>
                    GameHelper.GetPlayerAssets(pl.Id, pl.Money, ra.Cells, true)).ToArray());
            }
            return res;
        }

        public static List<string> ReadFromFile()
        {
            var dir = System.Web.Hosting.HostingEnvironment.MapPath("~/res");

            var fpath = Path.Combine(dir, "auc.txt");

            return File.ReadAllLines(fpath).OrderBy(x => x).ToList();
        }

        public static void SaveAucRules(List<GameLogic.AucRule> list)
        {
            var res = new List<string>();
            res.Add("//" + DateTime.Now.ToString());

            foreach (var rr in list.OrderBy(x=>x.GroupId))
            {
                var rule = string.Format("fac={0};gid={1};myc={2};anc={3};nb={4};money={5}",
                    rr.Factor, rr.GroupId, rr.MyCount, rr.AnCount, rr.GroupsWithHouses,
                    rr.MyMoney
                    );
                res.Add(rule);
            }
            var dir = System.Web.Hosting.HostingEnvironment.MapPath("~/res");
            var fpath = Path.Combine(dir, "auc.txt");

            File.WriteAllLines(fpath, res);
        }
    }
}