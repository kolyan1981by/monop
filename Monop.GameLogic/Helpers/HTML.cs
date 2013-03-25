using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogic
{
    public static class HTML
    {
        public const string TIMEOUT_TEXT = "<span style=\"color: red\">{0}</span>";
        public const string HTML_PLAYER_NAME = "<span class=\"p{0}\">{1}</span> &nbsp;";

        public static string PrintMoney(this int money)
        {
            var text= money >= 1000000 ? money / 1000000d + "M" : money / 1000d + "K";
            return text;
        }

        public static string PrintWithColor(this int money)
        {
            var text = PrintMoney(money);
            return string.Format("<span style=\"color: red\">{0}</span>", text);
        }
        
        public static string PrintRoll(this int[] roll)
        {
            return string.Format("[{0}:{1}]", roll[0], roll[1]);
        }
        
        public static string PlName(int Id, string Name = "bot-", bool IsBot = true)
        {
            string arg = IsBot ? ("bot_" + Id) : Name;
            return string.Format("<span class=\"p{0}\">{1}</span> &nbsp;", Id, arg);
        }

    }
}
