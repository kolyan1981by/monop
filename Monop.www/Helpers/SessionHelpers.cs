using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Monop.Forum;

namespace Monop.www.Helpers
{
    public class SessionHelper
    {
         
        //en-US , ru-RU
        public static string Locale
        {
            get
            {
                var sessCulture = HttpContext.Current.Session["Culture"];
                if (sessCulture == null) return Const.DEF_LOCALE;
                else return sessCulture as string;
            }
            set
            {
                HttpContext.Current.Session["Culture"] = value;
            }
        }
        internal static Dictionary<string, string> Cache
        {
            get
            {
                var cache = HttpContext.Current.Cache[Const.CacheSiteTexts] as Dictionary<string, string>;
                if (cache == null)
                {
                    HttpContext.Current.Cache[Const.CacheSiteTexts] = cache = InitCache();

                }
                return cache;
            }
        }

        private static Dictionary<string, string> InitCache()
        {
            if (ConfigHelper.EnableDB)
            {
                return DBService.Data.Query<cnfSiteText>("select * from cnfSiteTexts").ToDictionary(x => x.TextId, x => x.Text);
            }
            else { return new Dictionary<string, string>(); }

        }

        public static void AddOrUpdateCachedText(string TextId, string textEN, string textRU)
        {

            var k1 = MakeKey(TextId, Const.EN);
            var k2 = MakeKey(TextId, Const.RU);

            var texts = new[] { textEN, textRU };

            var db = DBService.Data;
            int i = 1;
            foreach (var key in new[] { k1, k2 })
            {
                var stext = db.SingleOrDefault<cnfSiteText>("where textid=@0", key);
                var text = texts[i - 1];
                //insert local text
                if (stext == null)
                {
                    stext = new cnfSiteText();
                    stext.TextId = key;
                    stext.Text = text;
                    stext.SiteLangId = i;
                    db.Insert(stext);
                }
                else
                {
                    stext.Text = text;
                    db.Update(stext);
                }
                Cache[key] = text;
                i++;
            }

        }

        private static string MakeKey(string TextId, string locale)
        {
            return TextId.ToLower() + "." + locale.ToLower();
        }

        internal static void SaveText()
        {


        }

        public static string GetText(string TextId)
        {

            var tt = GetCachedText(TextId);

            if (Locale == "en-US") return tt[0];
            if (Locale == "ru-RU") return tt[1];

            return tt[0];

        }

        public static string[] GetCachedText(string TextId)
        {

            var cache = Cache;

            string res0;
            string res1;

            if (cache.TryGetValue(MakeKey(TextId, Const.EN), out  res0)) { }

            if (cache.TryGetValue(MakeKey(TextId, Const.RU), out  res1)) { }

            return new[] { res0, res1 };

        }
    }
}
