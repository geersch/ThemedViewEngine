using System;
using System.Web;
using System.Globalization;
using CGeers.WindDirection.Managers;

namespace MvcApplication
{
    public class ThemeHttpModule : IHttpModule
    {
        public void Init(HttpApplication application)
        {
            application.BeginRequest += application_BeginRequest;
        }

        private void application_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication application = (HttpApplication) sender;
            HttpContext context = application.Context;
            if (context.Cache == null)
            {
                return;
            }
            
            string domain = context.Request.Url.GetDomain();

            string cacheKey = String.Format(CultureInfo.InvariantCulture, "theme_for_{0}", domain);

            if (context.Cache[cacheKey] == null)
            {
                ResellerManager manager = new ResellerManager();
                string theme = manager.GetThemeForDomain(domain);
                context.Cache[cacheKey] = theme;
            }
        }

        public void Dispose() { }
    }
}
