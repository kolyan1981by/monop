using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;

namespace GameLogic
{
    public class Simulator
    {
        public static Game RunFromXML(string xml, string ManagerFolderPath)
        {
            var g = GameHelper.RestoreGameFromXML(xml);
            InitWithData(g, ManagerFolderPath);
            g.conf.LifeTimerPeriod = 10;
            g.StartGame();

            while (!g.IsFinished)
            {
                Thread.Sleep(100);
            }
            return g;
        }

        public static Game Run(string ManagerFolderPath)
        {
            var g = GameHelper.CreateNew(2, 30, "en-US", 1, false);
            g.conf.LifeTimerPeriod = 10;
            g.conf.EnableLog = false;
            AddPlayers(g, 3);
            InitWithData(g, ManagerFolderPath);

            g.StartGame();

            while (!g.IsFinished)
            {
                Thread.Sleep(100);
            }

            return g;
        }

        public static string[] GetResult(Game g)
        {
            if (g.IsFinished)
                return new[] { g.Winner.htmlName, ShowTrades(g), ShowPlayersCells(g) };
            else return null;
        }


        static string ShowTrades(Game g)
        {

            var res = "";
            foreach (var tr in g.CompletedTrades)
            {
                var giveCells = string.Join(",", tr.give_cells.Select(x => g.Cells[x].Name).ToArray());
                var getCells = string.Join(",", tr.get_cells.Select(x => g.Cells[x].Name).ToArray());
                res += string.Format("<br /> {0}[give={1}]  {2} [give={3}], exchId={4},  reversed={5}",
                    tr.from.htmlName, giveCells, tr.to.htmlName, getCells, tr.ExchId, tr.Reversed);
            }
            return res;
        }

        static string ShowPlayersCells(Game g)
        {
            var res = "<table><tr>";
            foreach (var pl in g.Players)
            {
                res += string.Format("<td valign='top'>{0}<br />", pl.Name);
                var cells = g.Map.CellsByUser(pl.Id).OrderBy(x => x.Group);
                foreach (var cc in cells)
                {
                    res += string.Format("{0}{1}<br />", cc.Name, cc.IsMortgage ? "(M)" : "");
                }
                res += "</td>";
            }
            return res + "</tr></table>";
        }

        public static IEnumerable<string> GetLog(Game g,int countLines)
        {
            string filename = Path.Combine(Path.GetTempPath(), "log.htm");

            var count = g.Logs.Count();

            if (2 * countLines < count)
                return g.Logs.Take(countLines).Union(new[] { "************************" }).Union(g.Logs.Skip(count - countLines));
            else return g.Logs;

        }

        public static void AddPlayers(Game g, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var botName = "bot-" + i;
                g.Players.Add(new Player { Id = i, Name = botName, OneDirection = true, IsBot = true, Status = "master", Money = 15000000 });
            }
            //g.OnLeavedGame += (s, w) => { GameAction.SaveToDB(s, g, w); };
        }

        public static string[] GetGameActions(Game g)
        {
            if (g.IsFinished)
            {
                return new string[]
				{
					g.Winner.htmlName,
					Simulator.ShowTrades(g),
					Simulator.ShowPlayersCells(g)
				};
            }
            return null;
        }

        private static void InitWithData(Game g, string path)
        {
            g.InitRules(path);

            //string path2 = Path.Combine(path, "res_23.xml");
            //g.Texts = GameHelper.RestoreTexts(File.ReadAllText(path2));

            //var ui_xml = File.ReadAllText(Path.Combine(path, "ui.xml"));
            //g.UIParts = GameHelper.LoadUI(g, ui_xml);
        }
    }
}
