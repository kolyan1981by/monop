using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Configuration;


namespace Monop.Data
{
    public class GameRepo
    {
        public static void SaveState(string CurrUser, string gState, bool IsWinner)
        {
            //var _db = db;
            //var pr = _db.GameProfiles.FirstOrDefault(x => x.UserName.ToLower() == CurrUser.ToLower());
            //if (pr == null)
            //{
            //    pr = new GameProfile();
            //    _db.GameProfiles.InsertOnSubmit(pr);
            //    pr.UserName = CurrUser;
            //    pr.CountOfWin = 0;
            //    pr.CountOfLost = 0;
            //}
            //if (IsWinner)
            //    pr.CountOfWin++;
            //else
            //    pr.CountOfLost++;

            //_db.SubmitChanges();

        }

        public static List<string> GetTop()
        {
            //var _db = db;

            //return _db.GameProfiles.OrderByDescending(x => x.CountOfWin).Take(10)
            //    .Select(x => string.Format("{0} wins {1}", x.UserName, x.CountOfWin)).ToList();
            return null;
        }

        public static string GetPlayerStatus(string uname)
        {
            //var _db = db;
            //var p = _db.GameProfiles.FirstOrDefault(x => x.UserName == uname);

            //if (p != null)
            //{

            //    var kk = p.CountOfWin - p.CountOfLost;
            //    if (p.CountOfLost != 0)
            //        if (p.CountOfWin / (decimal)p.CountOfLost > 5) return "master";

            //    if (kk > 10) return "master";
            //}
            return "Beginner";
        }
    }
}
