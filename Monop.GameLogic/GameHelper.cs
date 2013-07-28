using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;
using System.Reflection;

namespace GameLogic
{
    public static class GameHelper
    {
        //job_inteval in milisec
        public static Game CreateNew(int max_players = 2, int timeout = 30,
            string locale = "en-US", int rollmode = 1, bool debugMode = false,int job_interval=10)
        {
            var g = new Game();
            g.conf.cnfMaxPlayers = max_players;
            g.conf.cnfTimeOut = timeout;
            g.conf.cnfGameLang = locale;
            g.conf.cnfRollMode = rollmode;
            g.conf.DebugMode = debugMode;
            g.conf.LifeTimerPeriod = job_interval;
            return g;
        }

        public static Player GetPlayer(this Game g, int id)
        {
            return g.Players.SingleOrDefault(x => x.Id == id);
        }

        public static Player GetPlayer(this Game g, string userName)
        {
            return g.Players.SingleOrDefault(x => x.Name == userName);
        }

        public static int GetPlayerAssets(this Game g, int pid, bool needMonop = true)
        {

            var money = g.GetPlayer(pid).Money;

            return GetPlayerAssets(pid, money, g.Cells, needMonop);
           
        }
        public static int GetPlayerAssets(int pid, int money, CellInf[] cells, bool needMonop = true)
        {
            var userCells = cells.Where(x => x.Owner == pid && !x.IsMortgage);

            int sum = 0;
            foreach (var item in userCells)
            {
                if (needMonop)
                {
                    sum += item.MortgageAmount;
                    if (item.HousesCount > 0)
                    {
                        sum += item.HousesCount * item.HouseCostWhenSell;
                    }
                }
                else
                {
                    if (!item.IsMonopoly) sum += item.Cost;
                }
            }
            return sum + money;
        }

        public static int GetPlayerCash(this Game g, int pid)
        {
            var cc = g.Map.CellsByUser(pid).Where(x => !x.IsMortgage);
            int sum = 0;
            foreach (var item in cc)
            {

                sum += item.MortgageAmount;
                if (item.HousesCount > 0)
                {
                    sum += item.HousesCount * item.HouseCostWhenSell;
                }

            }
            return sum + g.GetPlayer(pid).Money;
        }

       

        #region Init

        public static void InitRules(this Game g, string path)
        {
            g.InitTradeRules(path);
            g.InitAucRules(path);
        }

        public static void InitAucRules(this Game g, string path)
        {
            g.AucRules = ManualLogicHelper.LoadAucRules(File.ReadAllLines(Path.Combine(path, "auc.txt"))).ToList();
        }

        public static void InitTradeRules(this Game g, string path)
        {
            var rules = ManualLogicHelper.LoadExchanges(path).ToList();
            g.SetPlayerTradeRules(rules, -1);

        }

       


        #endregion

        #region Load lands

        public static IEnumerable<CellInf> LoadLands()
        {
            var text = GetResource("Monop.GameLogic.lands.txt");//FileHelper.openText(path);
            var records = GetFileRecords(text);

            foreach (var rec in records)
            {
                var cc = rec.Split('|').Select(x => x.Trim()).ToArray();

                yield return new CellInf
                {
                    Name = cc[0],
                    Id = cc[1] == "*" ? -1 : int.Parse(cc[1]),
                    Cost = cc[2] == "*" ? default(int) : int.Parse(cc[2]),
                    Type = int.Parse(cc[3]),
                    Group = cc[4] == "*" ? 0 : int.Parse(cc[4]),
                    ArendaInfo = cc[5] == "*" ? default(string) : cc[5],
                };

            }

        }

        static IEnumerable<string> GetFileRecords(string text)
        {
            var records = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
            foreach (var item in records.Skip(1))
            {
                if (!String.IsNullOrWhiteSpace(item))
                    yield return item;
            }
        }


        static string GetResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (var textReader = new StreamReader(assembly.GetManifestResourceStream(resourceName)))
            {
                return  textReader.ReadToEnd();
            } 
        }


        #endregion

        #region Save and Restore

        public static string RenderLastStateXML(Game g)
        {

            var gg = new XElement("Game");

            var plName = string.Join(";", g.Players.Select(x => x.Name).ToArray());

			gg.Add(new XAttribute("gid", g.Id));
			gg.Add(new XAttribute("Players", plName));
            gg.Add(new XAttribute("CurrId", g.Selected));
            gg.Add(new XAttribute("GameState", g.State));
            gg.Add(new XAttribute("RollMode", g.conf.cnfRollMode));
            gg.Add(new XAttribute("TimeOut", g.conf.cnfTimeOut));
            gg.Add(new XAttribute("Round", g.RoundNumber));

            var pp = new XElement("Players");
            foreach (var p in g.Players)
            {

                var pl = new XElement("Player");
                pl.Add(new XAttribute("PlayerId", p.Id));
                pl.Add(new XAttribute("money", p.Money));
                pl.Add(new XAttribute("pos", p.Pos));
                pl.Add(new XAttribute("isBot", p.IsBot));

                if (p.PlayerSteps.Count() > 3)
                {
                    var ps = p.PlayerSteps;
                    var count = p.PlayerSteps.Count();
                    var lastroll = string.Format("{0}:{1}:{2}", ps[count - 3], ps[count - 2], ps[count - 1]);
                    pl.Add(new XAttribute("lastRolls", lastroll));
                }

                var pCells = g.Cells.Where(x => x.Owner == p.Id).ToList();
                var cc = new XElement("Cells");
                foreach (var pCell in pCells)
                {
                    var cellInfo = string.Format("{0},{1}", pCell.HousesCount, pCell.IsMortgage ? 1 : 0);
                    cc.Add(new XElement("Cell", new XAttribute("cid", pCell.Id), cellInfo));
                }
                pl.Add(cc);
                //add player 

                pp.Add(pl);

            }
            // add players
            gg.Add(pp);

            //add log records
            gg.Add(RenderLog(g));

            return gg.ToString();
        }

        private static XElement RenderLog(Game g)
        {
            var log = new XElement("Log");

            foreach (var grRec in g.Logs.GroupBy(x => x.Substring(0, x.IndexOf(':'))))
            {
                var rec = string.Join(",", grRec.Select(x => string.Format("[{0}]", x)));
                log.Add(new XElement("Line", new XAttribute("round", grRec.Key), rec));
            }
            return log;
        }

        public static Game RestoreGameFromXML(string xml)
        {
            var xdoc = XDocument.Parse(xml);

            var g = new Game();

			g.Id = Guid.Parse(xdoc.Root.Attribute("gid").Value);

			g.Selected = Int32.Parse(xdoc.Root.Attribute("CurrId").Value);
            g.RunningGame = true;
            g.conf.cnfRollMode = Int32.Parse(xdoc.Root.Attribute("RollMode").Value);
            g.conf.cnfTimeOut = Int32.Parse(xdoc.Root.Attribute("TimeOut").Value);
            g.RoundNumber = Int32.Parse(xdoc.Root.Attribute("Round").Value);

            var pNames = xdoc.Root.Attribute("Players").Value.Split(';');
            int ids = 0;
            foreach (var pl in xdoc.Descendants("Player"))
            {
                var p = new Player();

                p.Id = Int32.Parse(pl.Attribute("PlayerId").Value);
                p.Name = pNames[p.Id];
                p.Money = Int32.Parse(pl.Attribute("money").Value);
                p.Pos = Int32.Parse(pl.Attribute("pos").Value);
                p.IsBot = pl.Attribute("isBot").Value == "true";
                p.Status = "beginner";

                var plSteps = pl.Attribute("lastRolls") == null ? "" : pl.Attribute("lastRolls").Value;
                p.PlayerSteps = string.IsNullOrWhiteSpace(plSteps) ? new List<int>() : plSteps.Split(':').Select(x => Int32.Parse(x)).ToList();

                if (p.PlayerSteps.Count() > 2)
                    p.LastRoll = new[] { p.PlayerSteps[2] / 10, p.PlayerSteps[2] % 10 };

                g.Players.Add(p);

                InitLandsWhenRestoreGame(g, p, pl);

                ids++;
            }


            return g;
        }

        private static void InitLandsWhenRestoreGame(Game g, Player p, XElement pl)
        {
            foreach (var cell in pl.Descendants("Cell"))
            {
                var cc = g.Cells[Int32.Parse(cell.Attribute("cid").Value)];
                cc.Owner = p.Id;
                //house count
                var value = cell.Value.Split(',');
                cc.HousesCount = Int32.Parse(value[0]);
                cc.IsMortgage = Int32.Parse(value[1]) == 1;
            }
            g.Map.UpdateMap();
        }


        #endregion

     
        public static string RenderGameTexts(Game g)
        {
            XElement xElement = new XElement("GameTexts");
            foreach (var current in g.Texts.OrderBy(x => x.Key))
            {
                XElement xElement2 = new XElement("text");
                xElement2.Add(new XAttribute("key", current.Key));
                xElement2.Add(new XElement("ru", current.Value[0]));
                xElement2.Add(new XElement("en", current.Value[1]));
                xElement.Add(xElement2);
            }
            return xElement.ToString();
        }

        public static Dictionary<string, string[]> RestoreTexts(string xml)
        {
            XDocument xDocument = XDocument.Parse(xml);
            return xDocument.Descendants("text").ToDictionary((XElement x) => x.Attribute("key").Value.ToLower(), (XElement x) => new string[]
			{
				x.Element("ru").Value,
				x.Element("en").Value
			});
        }
    }


    #region Classes

    public class GameConfig
    {
        public GameConfig()
        {
            cnfRollMode = 0;
            cnfGameLang = "en-US";
            cnfCreateTime = DateTime.Now;
            DebugMode = false;
        }

        // 0 - manual ; 1- random
        public int cnfRollMode { get; set; }

        //ru-RU,en-US
        public string cnfGameLang { get; set; }

        public int cnfTimeOut { get; set; }

        public int cnfMaxPlayers { get; set; }

        public bool DebugMode { get; set; }

        public DateTime cnfCreateTime { get; set; }


        public bool IsExtendedLog { get; set; }
        public int LifeTimerPeriod = 1000;

        public bool EnableLog = true;
    }

    public class Trade
    {
        public Player from;
        public Player to;
        public int[] give_cells;
        public int[] get_cells;
        public int giveMoney;
        public int getMoney;

        public bool Equals(Trade tr)
        {
            bool res = false;

            bool pls = from == tr.from && to == tr.to;

            bool lands1 = give_cells.Intersect(tr.give_cells).Count() == give_cells.Count();
            bool lands2 = get_cells.Intersect(tr.get_cells).Count() == get_cells.Count();
            bool money = getMoney == tr.getMoney && giveMoney == tr.giveMoney;

            if (pls && lands1 && lands2 && money) res = true;

            return res;
        }
        public override string ToString()
        {
            var from_to = string.Format("from:{0} to:{1} ", from.Name, to.Name);
            var give_get = string.Format("give:[{0},money={1}] get:[{2},money={3}]",
                string.Join(",", give_cells), giveMoney, string.Join(",", get_cells), getMoney);

            return from_to + give_get;
        }

        public int ExchId { get; set; }

        public bool Reversed { get; set; }
    }

    public class AuctionState
    {
        public AuctionState()
        {
            IsFinished = false;
        }
        public CellInf cell;
        public int currBid;
        public int nextBid { get { return currBid + 50000; } }

        public Player CurrPlayer { get; set; }
        public Player LastBiddedPlayer { get; set; }

        public bool IsFinished = false;
    }

    public class ChestCard
    {
        public ChestCard()
        {
            Type = 6;
        }
        public int Type { get; set; }
        public int RandomGroup { get; set; }
        public string Text { get; set; }
        public int Money { get; set; }
        public int Pos { get; set; }
    }

    public enum GameState
    {
        BeginStep,

        CanBuy,
        Auction,
        NeedPay,
        CantPay,

        Mortgage,
        Trade,
        Build,

        Police,
        MoveToCell,
        RandomCell,

        EndStep,
        FinishGame,
    }
    #endregion


}
