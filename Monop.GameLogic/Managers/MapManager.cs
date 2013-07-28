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

		#region InitMap

		public void InitMap()
		{
			InitLandCells();
			InitRandomCells();
		}

		public void InitLandCells()
		{
			g.Cells = GameHelper.LoadLands().OrderBy(x => x.Id).ToArray();
		}

		public void InitRandomCells()
		{
			var CommunityChest = new List<ChestCard>();
			var ChanceChest = new List<ChestCard>();

			g.CommunityChest = CommunityChest;
			g.ChanceChest = ChanceChest;

			//pay

			CommunityChest.Add(new ChestCard { RandomGroup = -1, Text = "need pay bank", Money = 100000 });
			//get money
			CommunityChest.Add(new ChestCard { RandomGroup = 1, Text = "get money 100k", Money = 100000 });
			CommunityChest.Add(new ChestCard { RandomGroup = 1, Text = "get money 1.5M", Money = 1500000 });
			CommunityChest.Add(new ChestCard { RandomGroup = 1, Text = "get money 2M", Money = 2000000 });

			//You are assessed for street repairs – $40 per house, $115 per hotel
			CommunityChest.Add(new ChestCard
			{
				RandomGroup = 15,
				Text = string.Format("You are assessed for street repairs – $100K per house, $400K per hotel"),
				Money = 0
			});


			CommunityChest.Add(new ChestCard { RandomGroup = 5, Text = "Get out of jail free" });
			CommunityChest.Add(new ChestCard { RandomGroup = 2, Text = "go to trans", Pos = 5 });

			//go to cell
			ChanceChest.Add(new ChestCard { RandomGroup = 2, Text = "Advance to Go", Pos = 0 });
			ChanceChest.Add(new ChestCard { RandomGroup = 2, Text = "go to Police", Pos = 10 });
			ChanceChest.Add(new ChestCard { RandomGroup = 2, Text = "go to", Pos = 11 });
			ChanceChest.Add(new ChestCard { RandomGroup = 2, Text = "go to", Pos = 24 });
			ChanceChest.Add(new ChestCard { RandomGroup = 2, Text = "go to ", Pos = 39 });

			ChanceChest.Add(new ChestCard { RandomGroup = 3, Text = "go to 3 cell back", Pos = 3 });

			ChanceChest.Add(new ChestCard { RandomGroup = 4, Text = "Pay each player $500K", Money = 500000 });

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
