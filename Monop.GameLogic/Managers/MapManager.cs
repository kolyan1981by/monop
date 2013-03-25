using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    public class MapManager
    {
        Game g;

        #region ctor

        public MapManager(Game game)
        {
            g = game;
        }

        #endregion


        #region Cells props

        public List<CellInf> CellsByUserByGroup(int pid, int group)
        {
            return g.Cells.Where(x => x.Owner == pid && x.Group == group).ToList();
        }

        public List<CellInf> CellsByUserByType(int pid, params int[] types)
        {

            return g.Cells.Where(x => x.Owner == pid && types.Contains(x.Type)).ToList();

        }
        public IEnumerable<CellInf> CellsByType(params int[] types)
        {

            return g.Cells.Where(x => types.Contains(x.Type));

        }
        public IEnumerable<CellInf> CellsByGroup(int group)
        {

            return g.Cells.Where(x => x.Group == group);

        }


        public IEnumerable<CellInf> CellsByUser(int pid)
        {
            return g.Cells.Where(x => x.Owner == pid);
        }

        #endregion


        public void SetOwner(Player p, CellInf cell)
        {
            cell.Owner = p.Id;
            cell.IsMortgage = false;
            p.Money -= cell.Cost;
            UpdateMap();
        }


        public void UpdateCell(CellInf cell)
        {
            var mm = g.Map.CellsByUserByGroup(cell.Owner.Value, cell.Group);
            cell.OwGrCount = mm.Count();
        }

        public void UpdateMap()
        {
            var groups = from x in CellsByType(1, 2, 3)
                         where x.Owner != null
                         group x by new { x.Group, x.Owner } into gg
                         select new { Key = gg.Key, Vals = gg };

            foreach (var group in groups)
            {
                var count = group.Vals.Count();

                foreach (var cell in group.Vals) cell.OwGrCount = count;
            }

        }

        public void TakeRandomCard()
        {
            //return g.CommunityChest.First(x => x.RandomGroup == 15);

            var c = g.CurrCell;
            var ChanceType = new[] { 7, 22, 36 }.Contains(c.Id);

            if (ChanceType)
            {
                var count = g.ChanceChest.Count;
                var r2 = new Random(DateTime.Now.Second).Next(count);
                g.LastRandomCard= g.ChanceChest[r2];
            }
            else
            {
                var count = g.CommunityChest.Count;
                var r2 = new Random(DateTime.Now.Second).Next(count);
                g.LastRandomCard= g.CommunityChest[r2];
            }

        }

        public int[] GetHotelsAndHousesCount(int pid)
        {
            var cc = CellsByUserByType(pid, 1);
            var houses = cc.Count(x => x.HousesCount > 0 && x.HousesCount < 5);
            var hotels = cc.Count(x => x.HousesCount == 5);
            return new[] { hotels, houses };

        }
      

    }
}
