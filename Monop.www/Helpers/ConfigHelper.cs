using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Monop.www.Helpers
{
    public class ConfigHelper
    {
        internal static bool EnableDB
        {
            get
            {
                return ConfigurationManager.AppSettings["EnableDB"] == "true";
            }
        }
        public static string GameAdmins
        {
            get
            {
                return ConfigurationManager.AppSettings["admin_name"];
            }
        }
        public static string UIPartsFile
        {
            get
            {
                return "ui.xml";
            }
        }
        public static string ResDir
        {
            get
            {
                return "~/res";
            }
        }
        public static bool IsAdmin(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                return GameAdmins.Contains(name);
            else return false;
        }
    }
}