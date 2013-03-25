using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Globalization;

namespace GameLogic
{
    public class TradeRule
    {
        public int Id { get; set; }
        public bool Disabled { get; set; }

        public int MyCount { get; set; }
        public int GetLand { get; set; }
        public int GetCount { get; set; }
        public int GetMoney { get; set; }
        public int YourCount { get; set; }
        public int GiveLand { get; set; }
        public int GiveCount { get; set; }
        public int GiveMoney { get; set; }
        public double MoneyFactor { get; set; }



    }
    public class AucRule
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int MyCount { get; set; }
        public int AnCount { get; set; }
        public bool NeedBuildHouses { get; set; }
        public string GroupsWithHouses { get; set; }
        public int MyMoney { get; set; }
        public double Factor { get; set; }

    }
    public class ManualLogicHelper
    {
        const string fileName = "exchange.txt";

        public static void SaveToFile(TradeRule st, string path, string fileName = "exchange.txt")
        {
            //var p = Server.MapPath("~/manage");
            var fpath = Path.Combine(path, fileName);
            string res = "";
            res = string.Format("{0}-{1}-{2};{3}-{4}-{5};{6}-{7};{8};d={9}",
                st.GetLand, st.GetCount, st.MyCount,
                st.GiveLand, st.GiveCount, st.YourCount,
                st.GetMoney, st.GiveMoney,
                (st.MoneyFactor == 0 ? 1 : st.MoneyFactor),
                (st.Disabled ? 1 : 0)
                );

            using (var sw = File.AppendText(fpath))
            {
                sw.WriteLine(res);
            }

        }

        public static IEnumerable<TradeRule> LoadExchanges(string p, string fileName = "exchange.txt")
        {
            //var p = Server.MapPath("~/manage");
            var fpath = Path.Combine(p, fileName);
            int i = 0;
            foreach (var str in File.ReadAllLines(fpath))
            {

                if (str.StartsWith("///") || string.IsNullOrWhiteSpace(str)) continue;

                var arr = str.Split(';');
                var count = arr.Count();

                var st = new TradeRule();
                st.Id = i++;
                if (count > 1)
                {

                    //to me
                    var mm1 = arr[0].Split('-');
                    st.GetLand = Int32.Parse(mm1[0]);
                    st.GetCount = Int32.Parse(mm1[1]);
                    st.MyCount = Int32.Parse(mm1[2]);

                    //to you
                    var mm2 = arr[1].Split('-');
                    st.GiveLand = Int32.Parse(mm2[0]);
                    st.GiveCount = Int32.Parse(mm2[1]);
                    st.YourCount = Int32.Parse(mm2[2]);
                }
                //parse money
                if (count > 2)
                {
                    var mm3 = arr[2].Split('-');
                    st.GetMoney = Int32.Parse(mm3[0]);
                    st.GiveMoney = Int32.Parse(mm3[1]);
                }
                //parse money factor
                st.MoneyFactor = (count > 3) ? Double.Parse(arr[3]) : 1;

                if (count > 4)
                    st.Disabled = arr[4] == "d=1";

                if (count > 4 && !st.Disabled)
                    yield return st;

            }
        }

        public static IEnumerable<AucRule> LoadAucRules_Old(string[] list)
        {
            foreach (var rr in list.Where(x => x.Trim() != ""))
            {
                var arr = rr.Split('-');
                yield return new AucRule
                {
                    GroupId = Int32.Parse(arr[0]),
                    MyCount = Int32.Parse(arr[1]),
                    AnCount = Int32.Parse(arr[2]),
                    NeedBuildHouses = Int32.Parse(arr[3]) == 1,
                    GroupsWithHouses = Int32.Parse(arr[3]) == 1 ? "0" : "",
                    Factor = Double.Parse(arr[4], new CultureInfo("en-US")),//Double.Parse(arr[4]),
                };
            }

        }

        public static IEnumerable<AucRule> LoadAucRules(string[] list)
        {
            int i = 0;
            foreach (var rr in list.Where(x => x.Trim() != "" && !x.StartsWith("//")))
            {
                var arr = rr.Split(';');
                yield return new AucRule
                {
                    Id = i++,
                    GroupId = Int32.Parse(FindValue("gid=", arr)),
                    MyCount = Int32.Parse(FindValue("myc=", arr)),
                    AnCount = Int32.Parse(FindValue("anc=", arr)),
                    MyMoney = Int32.Parse(FindValue("money=", arr)),

                    GroupsWithHouses = FindValue("nb=", arr),

                    NeedBuildHouses = !string.IsNullOrEmpty(FindValue("nb=", arr)),
                    Factor = Double.Parse(FindValue("fac=", arr), new CultureInfo("en-US")),//Double.Parse(arr[4]),
                };
            }

        }

        private static string FindValue(string p, string[] arr)
        {
            var val = arr.FirstOrDefault(x => x.Trim().StartsWith(p));
            return val.Replace(p, "");
        }

    }
}
