using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Monop.www.Helpers
{
    public class ImageHelper
    {
        static string img = "<img src='/images/smiles/{0}.gif' alt=''>";

        static string[] sCodes = new string[] { "01", "02", "11a", "03a", "04b", "06a", "07a", "08a", "09a", "10a",
        "13a","47a","14a","26a","27a","28a","41a","44a","45a","46a",};

        public static string ProcessMessage(string mes)
        {
            var dict = new Dictionary<string, string>()
            {
                {":)","04b"},
                {":(","06a"},
                {";)","09a"},
            };
            //replace code with smile
            foreach (var item in sCodes)
            {
                mes = mes.Replace(item, string.Format(img, item));
            }
            foreach (var item in dict)
            {
                mes = mes.Replace(item.Key, string.Format(img, item.Value));
            }

            return mes;
        }

        static string[] rollCodes = new string[] { 
            "0.gif",
            "1.gif",
            "2.gif",
            "3.gif",
            "4.gif",
            "5.gif",
            "6.gif",
           };
        static string imgRoll = "<img src='/Content/images/{0}' alt=''>";

        public static string GetRollImage(int r)
        {
            return string.Format(imgRoll, rollCodes[r]);
        }
    }
}