using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for Player
/// </summary>
namespace GameLogic
{
    public class Player : ICloneable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public bool IsBot { get; set; }
        public bool InAuction { get; set; }
        public bool Deleted { get; set; }
        
        public string htmlName
        {
            get
            {
                var b = IsBot ? "(b)" : "";
                return string.Format(HTML.HTML_PLAYER_NAME, Id, Name + b);
            }
        }
        public bool OneDirection { get; set; }
        public bool IsCustomAuc { get; set; }

        public int Money { get; set; }
        public int Police { get; set; }
        public int FreePoliceKey { get; set; }

        public int Pos { get; set; }
        public int[] LastRoll { get; set; }
        public int ManRoll { get; set; }
        public bool EnableDoubleRoll { get; set; }


        public string PrintLastRoll
        {
            get
            {
                var r = LastRoll;
                if (r != null)
                    return r[0] + ":" + r[1];
                else return "";
            }
        }
        public bool CaughtByPolice
        {
            get
            {
                if (Pos == 10 && Police > 0) return true;
                else return false;
            }
        }

        public bool Step()
        {

            var r0 = LastRoll[0];
            var r1 = LastRoll[1];

            //if (PlayerSteps.Count() > 100) PlayerSteps = PlayerSteps.Skip(90).ToList();

            Pos += r0 + r1;
            PlayerSteps.Add(r0 * 10 + r1);

            if (CheckOnTriple())
            {
                Pos = 10;
                Police = 1;
                PlayerSteps.Clear();
                return false;
            }

            if (Pos >= 40)
            {
                Money += 2000000;
                Pos %= 40;
            }
            return true;

        }

        private bool CheckOnTriple()
        {

            if (PlayerSteps.Count() >= 3)
            {
                foreach (var rr in PlayerSteps.AsEnumerable().Reverse().Take(3))
                {
                    if (rr / 10 != rr % 10) return false;
                }
                return true;
            }
            else
            {
                return false;
            }

        }
        public List<int> PlayerSteps = new List<int>();

        public object Clone()
        {
            return base.MemberwiseClone();
        }
    }
}
