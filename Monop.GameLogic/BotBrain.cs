using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    public class BotBrain
    {

        public static bool ShouldGoFromPolice(Game g)
        {
            //free cells
            var f4 = g.Map.CellsByGroup(4).Where(x => x.Owner == null).Any();
            var f5 = g.Map.CellsByGroup(5).Where(x => x.Owner == null).Any();

            //monopoly cells
            var m4 = GroupIsNotMyMonopoly(g, 4).Any();
            var m5 = GroupIsNotMyMonopoly(g, 5).Any();
            var m6 = GroupIsNotMyMonopoly(g, 6).Any();
            var m7 = GroupIsNotMyMonopoly(g, 7).Any();

            if (m4 || m5)
                return false;

            if (f4 || f5) return true;

            if (m6)
                return false;

            return true;

        }

        static IEnumerable<CellInf> GroupIsNotMyMonopoly(Game g, int group)
        {
            return g.Cells.Where(x => x.Group == group && x.Owner != g.Curr.Id && x.IsMonopoly);
        }

        public static bool BeforeRollMakeBotAction(Game g)
        {
            var res = BotBrainTrade.TryDoTrade(g);
            return res;
        }

        public static void BotActionWhenFinishStep(Game g)
        {
            BotBrain.UnMortgageLands(g, g.Curr);

            BotBrainHouses.BuildHouses(g, g.Curr.Money, null);
        }


        #region Factor of Buy

        public static bool NeedBuy(Game g, Player p, CellInf cell)
        {
            var fact = BotBrain.FactorOfBuy(g, p, cell);

            bool needBuy = false;

            if (fact >= 100)
                needBuy = BotBrain.MortgageSell(g, p, cell.Cost);

            else if (fact >= 1)
                needBuy = BotBrain.Mortgage(g, p, cell.Cost);

            return needBuy;

        }

        public static double? GetManualFactor(IEnumerable<AucRule> rules, int gr_id, int myCount, int aCount, int[] groupsWithHouses)
        {
            foreach (var auc in rules)
            {
                // if rul is empty , mean that we 
                bool needBuild = string.IsNullOrEmpty(auc.GroupsWithHouses);

                if (!string.IsNullOrEmpty(auc.GroupsWithHouses)
                    && auc.GroupsWithHouses != "any")
                    needBuild = auc.GroupsWithHouses.Split(',')
                    .Select(x => Int32.Parse(x)).Intersect(groupsWithHouses).Any();
                else if (auc.GroupsWithHouses == "any") needBuild = groupsWithHouses.Any();

               
                if (auc.GroupId == gr_id
                    && auc.MyCount == myCount
                    && auc.AnCount == aCount
                    && needBuild)

                    return auc.Factor;
            }
            return null;

        }

        public static double FactorOfBuy(Game g, Player p, CellInf cell)
        {

            double isNeedBuy = 0;
           
            var groupsWithHouses = BotBrainHouses.GetGroupsWhereNeedBuildHouses(g, p);
            var needBuild = groupsWithHouses.Any();

            //--buy
            var gg = g.Map.CellsByGroup(cell.Group);

            //other monopoly
            var notMine = gg.Where(x => x.Owner.HasValue && x.Owner != p.Id);

            var myCount = gg.Count(x => x.Owner == p.Id);

            int aCount = 0;
            int? owPid = null;

            if (notMine.Any())
            {
                aCount = notMine.Max(x => x.OwGrCount);
                owPid = notMine.First(x => x.OwGrCount == aCount).Owner;
            }

            var manualFactor = GetManualFactor(g.AucRules, cell.Group, myCount, aCount, groupsWithHouses);
            if (manualFactor.HasValue) return manualFactor.Value;

            var cg = cell.Group;

            //if another monopoly
            if (aCount == 2 && owPid.HasValue)
            {
                var sum = g.GetPlayerCash(owPid.Value);
                if (cg == 2 && sum > 4000000) return 100;
                if (cg == 3 && sum > 4000000) return 100;
                if (cg == 4 && sum > 5000000) return 100;
                if (cg == 5 && sum > 7000000) return 100;


                if (cell.Type == 1) return 1.7;
            }

            if (needBuild) return 0;

            if (cg == 1 || cg == 8)
                isNeedBuy = Factor_1_8(cg, aCount);

            if (cg == 11 && !needBuild)
            {
                if (aCount > 2) isNeedBuy = 2;
                else
                    isNeedBuy = 1.5;

            }
            if (cg == 33)
                isNeedBuy = Factor_11_33(cg, aCount);


            //my second cell
            if (myCount == 1) isNeedBuy = 2.2;

            //my monopoly
            if (myCount == 2) isNeedBuy = 3;

            if (aCount == 1 && cell.Group != 1 && cell.Group != 8) isNeedBuy = 2;

            //first land
            if (gg.Count(x => x.Owner == null) == 3 && !needBuild)
                isNeedBuy = 1.4;

            return isNeedBuy;
        }



        private static double Factor_11_33(int cg, int aCount)
        {
            double isNeedBuy = 0;

            if (cg == 11)
            {
            }

            if (cg == 33)
            {
                if (aCount > 0) isNeedBuy = 1.4;
                else isNeedBuy = 1.1;
            }
            return isNeedBuy;
        }

        private static double Factor_1_8(int cg, int aCount)
        {
            double isNeedBuy = 0;

            if (cg == 1)
            {
                if (aCount == 1) isNeedBuy = 3;
                else
                    isNeedBuy = 2;
            }

            if (cg == 8)
            {
                if (aCount == 1) isNeedBuy = 3;
                else
                    isNeedBuy = 2;
            }
            return isNeedBuy;
        }

        #endregion

        public static bool Mortgage(Game g, Player p, int PayAmount, bool InclMon = false)
        {
            if (p.Money >= PayAmount) return true;

            var cells = g.Map.CellsByUser(p.Id).Where(a => !a.IsMortgage);

            //select non monopoly cells-type1
            var lands = cells.Where(x => !x.IsMonopoly && x.Type == 1);

            //select trans and power cells
            var transPower = cells.Where(x => x.Type == 2 || x.Type == 3);

            //select monopoly without houses
            IEnumerable<CellInf> q  = lands.Union(transPower);

            if (InclMon)
            {
                var landsMon000 = cells.Where(x => x.IsMonopoly).GroupBy(x => x.Group).
                Where(x => x.All(a => a.HousesCount == 0)).SelectMany(x => x);
                q = q.Union(landsMon000);
            }

            string text = "";

            foreach (var cell in q)
            {
                if (p.Money >= PayAmount) break;

                p.Money += cell.MortgageAmount;
                cell.IsMortgage = true;
                text = text + "_" + cell.Id;
            }

            if (!string.IsNullOrEmpty(text))
            {
                g.FixAction("mortgage" + text);
            }

            return (p.Money >= PayAmount);
        }

        public static bool MortgageSell(Game g, Player p, int PayAmount)
        {

            //p.Money -= PayAmount;
            if (p.Money >= PayAmount) return true;

            //1 - mortage non monopoly lands
            BotBrain.Mortgage(g, p, PayAmount);

            //2 - sell houses
            BotBrainHouses.SellHouses(g, PayAmount);

            //3 -mortage monopoly without houses
            BotBrain.Mortgage(g, p, PayAmount, true);

            return p.Money >= PayAmount;

        }

       
        

        public static void UnMortgageLands(Game g, Player p)
        {
            //if (!BotBrain.NeedBuildHouses(g)) return;

            var trans = g.Map.CellsByUserByGroup(p.Id, 11);

            string text = "";

            if (trans.Count() > 2)
                text += UnMortg(g, trans);

            var cellsMon = g.Map.CellsByUser(p.Id).Where(x => x.IsMortgage && x.IsMonopoly)
                .GroupBy(x => x.Group);

            if (cellsMon.Any())
            {
                //Mortgage(g, p, cellsMon.First().UnMortgageAmount);
                text += UnMortg(g, cellsMon.FirstOrDefault());
            }

            //var cells = g.Map.CellsByUser(p.Id).Where(x => x.IsMortgage);
            //UnMortg(g, cells);

            if (text != "")
            {
                g.FixAction("unmortgage" + text);
            }
        }

        private static string UnMortg(Game g, IEnumerable<CellInf> cells)
        {
            var p = g.Curr;
            string text = "";

            foreach (var c in cells.Where(x => x.IsMortgage))
            {
                var mort = c.UnMortgageAmount;

                if (p.Money < mort) return text;
                p.Money -= mort;
                c.IsMortgage = false;
                text = text + "_" + c.Id;
            }
            return text;
        }

        #region Pay Options

      

        #endregion
    }
}
