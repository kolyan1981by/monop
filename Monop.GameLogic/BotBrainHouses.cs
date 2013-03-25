using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    public class BotBrainHouses
    {

        public static bool NeedBuildHouses(Game g, Player p)
        {
            return g.Map.CellsByUser(p.Id).Where(x => x.Type == 1 && x.HousesCount > 0 && x.HousesCount < 4).Any();
        }

        public static int[] GetGroupsWhereNeedBuildHouses(Game g, Player p,int maxCount=3)
        {
            return g.Map.CellsByUser(p.Id)
                .Where(x => x.Type == 1 && x.HousesCount > 0 && x.HousesCount <= maxCount)
                .GroupBy(x=>x.Group).Select(x=>x.Key).ToArray();
        }
        
        public static void SellHouses(Game g, int PayAmount)
        {
            var p = g.Curr;

            var grCells = from x in g.Map.CellsByUser(p.Id)
                          where x.IsMonopoly && !x.IsMortgage
                          //group x by x.Value.Group into gg
                          //select new { gg.Key, Vals = gg };
                          select x;
            
            string text = "";

            while (grCells.Where(x => x.HousesCount > 0).Any())
            {
                foreach (var cell in grCells.OrderByDescending(x => x.HousesCount))
                {
                    if (p.Money >= PayAmount) return;

                    if (cell.HousesCount > 0)
                    {
                        cell.HousesCount--;
                        p.Money += cell.HouseCostWhenSell;
                        text = text + "_" + cell.Id;
                    }
                }
            }
            if (text != "")
            {
                g.FixAction("sell_houses" + text);
            }

        }

        public static void BuildHouses(Game g, int Sum, int? Group)
        {
            //if (!g.Curr.IsBot) return;

            var p = g.Curr;

            var groupCells = from x in g.Map.CellsByUserByType(p.Id, 1)
                             where x.IsMonopoly
                             group x by x.Group into gr
                             where gr.All(a => !a.IsMortgage)
                             select new { Key = gr.Key, Vals = gr };

            if (Group.HasValue)
                groupCells = groupCells.Where(x => x.Key == Group);

            //var amSpent = GetCashForHouses(g, p);

            string text = "";

            foreach (var gr in groupCells.
               OrderByDescending(x => x.Vals.Max(a => a.HousesCount)))
            {
                var cost = gr.Vals.First().HouseCost;

                while (BotBrain.Mortgage(g, p, cost) && gr.Vals.Any(c => c.HousesCount < 5))
                    foreach (var cell in gr.Vals.OrderBy(c => c.HousesCount).ThenByDescending(x => x.Id))
                    {
                        if (p.Money < cell.HouseCost)
                            BotBrain.Mortgage(g, p, cell.HouseCost);

                        if (p.Money >= cell.HouseCost && cell.HousesCount < 5)
                        {
                            cell.HousesCount++;
                            p.Money -= cell.HouseCost;
                            text = text + "_" + cell.Id;
                        }
                    }

            }
            if (text != "")
            {
                g.FixAction("build" + text);
            }
        }

        public static int GetCashForHouses(Game g, Player p)
        {
            var cells = g.Map.CellsByUser(p.Id).Where(a => !a.IsMortgage);

            var lands = cells.Where(x => !x.IsMonopoly && x.Type == 1);

            var landsMon000 = cells.Where(x => x.IsMonopoly).GroupBy(x => x.Group).
                Where(x => x.All(a => a.HousesCount == 0)).SelectMany(x => x);

            var transPower = cells.Where(x => x.Type == 2 || x.Type == 3);

            var sum = 0;

            foreach (var cell in lands.Union(transPower).Union(landsMon000))
            {
                sum += cell.MortgageAmount;
                cell.IsMortgage = true;
            }

            return sum;
        }
    }
}
