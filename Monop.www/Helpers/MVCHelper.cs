using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Text;
using System.Linq;
using Monop.www.Helpers;
using System.IO;
using System.Web.UI;

namespace Monop.www.Helpers
{

    public static class MVCHelper
    {

        public static string RenderPartialToString(string controlName, object viewData)
        {
            ViewDataDictionary vd = new ViewDataDictionary(viewData);
            ViewPage vp = new ViewPage { ViewData = vd };
            Control control = vp.LoadControl(controlName);

            vp.Controls.Add(control);

            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                using (HtmlTextWriter tw = new HtmlTextWriter(sw))
                {
                    vp.RenderControl(tw);
                }
            }

            return sb.ToString();
        }

        public static MvcHtmlString Span(this HtmlHelper helper, string id)
        {
            return helper.Span(id, id);
        }

        public static MvcHtmlString Span(this HtmlHelper helper, string id, string defText)
        {
            var text = SessionHelper.GetText(id);
            if (string.IsNullOrEmpty(text)) text = defText;

            var ctx = helper.ViewContext.RequestContext;

            if (ctx.HttpContext.Request.IsAuthenticated)
            {
                var uname = ctx.HttpContext.User.Identity.Name;
                if (ConfigHelper.IsAdmin(uname))
                {
                    if (String.IsNullOrEmpty(text)) text = defText;
                    return MvcHtmlString.Create(
                        String.Format("<span  id=\"{0}\" class=\"trans_element\">{1}</span>", id, text));
                }
            }
            return MvcHtmlString.Create(text);

        }

        public static string Text(this HtmlHelper helper, string id)
        {
            string tt = SessionHelper.GetText(id);
            return tt ?? id;
        }

        public static string Text(this HtmlHelper helper, string text_en_en, string text_ru_ru)
        {
            if (SessionHelper.Locale == "en-US")
            {
                return text_en_en;
            }
            else if (SessionHelper.Locale == "ru_RU")
            {
                return text_ru_ru;
            }

            return "no text";

        }

        #region Table

        public static string Table(this HtmlHelper helper, string name, IList items, IDictionary<string, object> attributes)
        {
            if (items == null || items.Count == 0 || string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            return BuildTable(name, items, attributes);
        }

        private static string BuildTable(string name, IList items, IDictionary<string, object> attributes)
        {
            StringBuilder sb = new StringBuilder();
            BuildTableHeader(sb, items[0].GetType());

            foreach (var item in items)
            {
                BuildTableRow(sb, item);
            }

            TagBuilder builder = new TagBuilder("table");
            builder.MergeAttributes(attributes);
            builder.MergeAttribute("name", name);
            builder.InnerHtml = sb.ToString();
            return builder.ToString(TagRenderMode.Normal);
        }

        private static void BuildTableRow(StringBuilder sb, object obj)
        {
            Type objType = obj.GetType();
            sb.AppendLine("\t<tr>");
            foreach (var property in objType.GetProperties())
            {
                sb.AppendFormat("\t\t<td>{0}</td>\n", property.GetValue(obj, null));
            }
            sb.AppendLine("\t</tr>");
        }

        private static void BuildTableHeader(StringBuilder sb, Type p)
        {
            sb.AppendLine("\t<tr>");
            foreach (var property in p.GetProperties())
            {
                sb.AppendFormat("\t\t<th>{0}</th>\n", property.Name);
            }
            sb.AppendLine("\t</tr>");
        }

        #endregion
    }

}
