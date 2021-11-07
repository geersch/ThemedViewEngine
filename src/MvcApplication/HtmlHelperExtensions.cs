using System;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Globalization;

namespace MvcApplication
{
    public static class HtmlHelperExtensions
    {
        public static string GetThemedStyleSheet(this HtmlHelper html)
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
            {
                throw new InvalidOperationException("Http Context cannot be null.");
            }

            string defaultStyleSheet = context.Server.MapPath("~/Themes/Default/Content/Site.css");

            string domain = context.Request.Url.GetDomain();
            string cacheKey = String.Format(CultureInfo.InvariantCulture, "theme_for_{0}", domain);
            string theme = (string) context.Cache[cacheKey];
            if (String.IsNullOrEmpty(theme) || theme == "Default")
            {
                return defaultStyleSheet;
            }
            
            string styleSheet = context.Server.MapPath(String.Format(CultureInfo.InvariantCulture,
                "~/Themes/{0}/Content/Site.css", theme));
            if (!File.Exists(styleSheet))
            {
                styleSheet = defaultStyleSheet;
            }
            return String.Format(CultureInfo.InvariantCulture, "'{0}'", styleSheet);
        }
    }
}
