using GameLogic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace Monop.www.Helpers
{
    public class SiteGameHelper
    {

        internal static void SaveToDB(string uname, Game g, bool isWinner)
        {
            if (ConfigHelper.EnableDB)
                Monop.Data.GameRepo.SaveState(uname, "gamestate", isWinner);
        }

        public static void LoadResources(Game g, string path)
        {
            //InitManualLogic
            g.InitRules(path);
        }
       
    }
}