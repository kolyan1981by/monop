using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using GameLogic;

namespace Monop.www.Helpers
{

    public class MapHelper
    {
        Table Map;

        string tip = "<a class=\"cellInfo\" href=\"#\" title=\"{1}|{2}\">{0}</a>";

        public static Dictionary<int, string> GetPlayerState(List<Player> players)
        {
            var poss = from x in players
                       group x by x.Pos into gg
                       select new { pos = gg.Key, pl = gg.Select(a => a.Id.ToString()).ToArray() };


            return poss.ToDictionary(
               x => x.pos,
               x => string.Join("", x.pl.Select(xx => string.Format("<img src='/Content/images/p/p{0}.png' >", xx)).ToArray())
               );

        }

        private string ClueTipText(int t, string inf)
        {
            var hh = inf.Split(';');
            var t1 = "rent {0}<br /> 1 house = {1} <br /> 2 house = {2} <br /> 3 house = {3} <br /> 4 house = {4} <br /> hotel = {5} <br />";
            if (t == 1)
            {
                return string.Format(t1, hh[0], hh[1], hh[2], hh[3], hh[4], hh[5]);
            }
            return t1;
        }



        private static Color GetColorOfGroup(int? p)
        {
            if (p == 1) return Color.LightGreen;
            if (p == 2) return Color.FromArgb(0, 204, 255);
            if (p == 3) return Color.FromArgb(153, 51, 102);
            if (p == 4) return Color.FromArgb(255, 204, 51);
            if (p == 5) return Color.Red;
            if (p == 6) return Color.FromArgb(105, 216, 33);
            if (p == 7) return Color.FromArgb(229, 103, 23);
            if (p == 8) return Color.DarkBlue;
            if (p == 11) return Color.Gray;
            if (p == 22) return Color.FromArgb(150, 150, 150);
            return Color.FromArgb(51, 102, 102);

        }
        public static string PrintPlayerCellText(CellInf cell)
        {
            var c = GetColorOfGroup(cell.Group);
            var CellName = string.Format(ColorGroupCell, c.R, c.G, c.B, cell.Name);
            var IsMortg = cell.IsMortgage ? "(M)" : "";
            var HousesCount = cell.PrintHouses;
            return string.Format("{0}{1}{2}", CellName, IsMortg, HousesCount);
        }

        public static string GetGroupColorCellName(int Group, string CellName)
        {
            var c = GetColorOfGroup(Group);
            return string.Format(ColorGroupCell, c.R, c.G, c.B, CellName);
        }

        static string ColorGroupCell = "<SPAN style=\"color:rgb({0}, {1}, {2})\">{3}</SPAN>";






        private string GetPlayerImage(string p)
        {
            return string.Format("/images/p/p{0}.png", p);
        }
        public static Color GetPlayerColor(int? p)
        {
            if (p == 0) return Color.FromArgb(255, 128, 128);
            if (p == 1) return Color.LightBlue;
            if (p == 2) return Color.LightGreen;
            if (p == 3) return Color.Yellow;
            if (p == 10) return Color.Gray;
            return Color.White;

        }

        public static string GetPlayerColorRGB(int? p)
        {
            if (p == 0) return "#FF8080";
            if (p == 1) return "LightBlue";
            if (p == 2) return "LightGreen";
            if (p == 3) return "Yellow";
            if (p == 10) return "Gray";
            return "White";

        }
    }

}