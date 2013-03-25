using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PetaPoco;
using Monop.Forum;

namespace Monop.Forum
{
    public class DBService
    {
        public static PetaPoco.Database Data
        {
            get
            {
                return new PetaPoco.Database("ApplicationServices");
            }
        }
        public static void LoqReq(System.Web.HttpRequestBase Request)
        {
            var db = DBService.Data;
            var rec = new SiteLog();
            var addr = Request.ServerVariables["REMOTE_ADDR"];
            var user = Request.ServerVariables["HTTP_USER_AGENT"];
            var referer = Request.ServerVariables["http_referer"];
            string RawUrl = Request.RawUrl;

            rec.ReqestIp = addr;
            
            rec.Date = DateTime.Now;

            rec.UserAgent = CheckLen(user,130);
            rec.RequestedUrl = CheckLen(RawUrl, 100); 
            rec.Referer = CheckLen(referer, 200); 

            db.Insert(rec);

        }

        private static string CheckLen(string str,int len)
        {
            if (!string.IsNullOrWhiteSpace(str) && str.Length > len)
                return str.Substring(str.Length - len, len);
            else return str;
        }


        public static void LoqReq(System.Web.HttpRequest Request)
        {
            var db = DBService.Data;
            var rec = new SiteLog();
            var addr = Request.ServerVariables["REMOTE_ADDR"];
            var user = Request.ServerVariables["HTTP_USER_AGENT"];
            var referer = Request.ServerVariables["http_referer"];
            string RawUrl = Request.RawUrl;

            rec.ReqestIp = addr;

            rec.Date = DateTime.Now;

            rec.UserAgent = CheckLen(user, 130);
            rec.RequestedUrl = CheckLen(RawUrl, 100);
            rec.Referer = CheckLen(referer, 200);

            db.Insert(rec);

        }
    }
}
