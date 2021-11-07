using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MvcApplication
{
    // http://pietschsoft.com/post/2009/03/ASPNET-MVC-Implement-Theme-Folders-using-a-Custom-ViewEngine.aspx
    public class ThemedViewEngine : WebFormViewEngine
    {
        #region Constructor(s)

        // Replace the default search paths by our own.
        public ThemedViewEngine()
        {
            // Search paths for the master pages
            base.MasterLocationFormats = new[]
                                             {
                                                 "~/Themes/{2}/Views/{1}/{0}.master",
                                                 "~/Themes/{2}/Views/Shared/{0}.master"
                                             };

            // Search paths for the views
            base.ViewLocationFormats = new[]
                                           {
                                               "~/Themes/{2}/Views/{1}/{0}.aspx",
                                               "~/Themes/{2}/Views/{1}/{0}.ascx",
                                               "~/Themes/{2}/Views/Shared/{0}.aspx",
                                               "~/Themes/{2}/Views/Shared/{0}.ascx",
                                           };

            // Search parts for the partial views
            // The search parts for the partial views are the same as the regular views
            base.PartialViewLocationFormats = base.ViewLocationFormats;
        }

        #endregion

        #region Helper Methods

        private static string GetTheme()
        {
            HttpContext context = HttpContext.Current;
            if (context == null)
            {
                throw new InvalidOperationException("Http Context cannot be null.");
            }

            string domain = context.Request.Url.GetDomain();
            string cacheKey = String.Format(CultureInfo.InvariantCulture, "theme_for_{0}", domain);
            string theme = (string) context.Cache[cacheKey];
            return theme;
        }

        private string GetPath(ControllerContext controllerContext, string[] locations, string name, 
            string theme, string controller, string cacheKeyPrefix, bool useCache, out string[] searchedLocations)
        {
            searchedLocations = new string[] { };
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }
            if ((locations == null) || (locations.Length == 0))
            {
                throw new InvalidOperationException("locations must not be null or emtpy.");
            }

            bool flag = IsSpecificPath(name);
            string key = this.CreateCacheKey(cacheKeyPrefix, name, flag ? string.Empty : controller, theme);
            if (useCache)
            {
                string viewLocation = this.ViewLocationCache.GetViewLocation(controllerContext.HttpContext, key);
                if (viewLocation != null)
                {
                    return viewLocation;
                }
            }
            if (!flag)
            {
                string path = this.GetPathFromGeneralName(controllerContext, locations, name, controller, theme, key, ref searchedLocations);
                if (String.IsNullOrEmpty(path))
                {
                    path = this.GetPathFromGeneralName(controllerContext, locations, name, controller, "Default", key, ref searchedLocations);
                }
                return path;
            }
            return this.GetPathFromSpecificName(controllerContext, name, key, ref searchedLocations);
        }

        private static bool IsSpecificPath(string name)
        {
            char firstCharacter = name[0];
            if (firstCharacter != '~')
            {
                return (firstCharacter == '/');
            }
            return true;
        }

        private string CreateCacheKey(string prefix, string name, string controllerName, string theme)
        {
            return string.Format(CultureInfo.InvariantCulture, ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}",
                new object[] { base.GetType().AssemblyQualifiedName, prefix, name, controllerName, theme });
        }

        private string GetPathFromGeneralName(ControllerContext controllerContext, string[] locations, string name,
            string controller, string theme, string cacheKey, ref string[] searchedLocations)
        {
            string virtualPath = string.Empty;
            searchedLocations = new string[locations.Length];
            for (int i = 0; i < locations.Length; i++)
            {
                string path = string.Format(CultureInfo.InvariantCulture, locations[i], new object[] { name, controller, theme });

                if (this.FileExists(controllerContext, path))
                {
                    searchedLocations = new string[] { };
                    virtualPath = path;
                    this.ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, virtualPath);
                    return virtualPath;
                }
                searchedLocations[i] = path;
            }
            return virtualPath;
        }

        private string GetPathFromSpecificName(ControllerContext controllerContext, string name, string cacheKey, ref string[] searchedLocations)
        {
            string virtualPath = name;
            if (!this.FileExists(controllerContext, name))
            {
                virtualPath = string.Empty;
                searchedLocations = new[] { name };
            }
            this.ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, virtualPath);
            return virtualPath;
        }

        #endregion

        #region Override Default Behavior

        protected override bool FileExists(ControllerContext controllerContext, string virtualPath)
        {
            try
            {
                return File.Exists(controllerContext.HttpContext.Server.MapPath(virtualPath));
            }
            catch (HttpException exception)
            {
                if (exception.GetHttpCode() != 0x194)
                {
                    throw;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (string.IsNullOrEmpty(partialViewName))
            {
                throw new ArgumentException("partialViewName");
            }
            string[] strArray;

            string theme = GetTheme();

            string requiredString = controllerContext.RouteData.GetRequiredString("controller");
            string partialViewPath = this.GetPath(controllerContext, this.PartialViewLocationFormats, partialViewName, theme, requiredString, "Partial", useCache, out strArray);
            if (string.IsNullOrEmpty(partialViewPath))
            {
                return new ViewEngineResult(strArray);
            }
            return new ViewEngineResult(this.CreatePartialView(controllerContext, partialViewPath), this);
        }

        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            if (controllerContext == null)
            {
                throw new ArgumentNullException("controllerContext");
            }
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException("viewName");
            }

            string[] strArray;
            string[] strArray2;

            string theme = GetTheme();

            string requiredString = controllerContext.RouteData.GetRequiredString("controller");

            string viewPath = this.GetPath(controllerContext, this.ViewLocationFormats,
                    viewName, theme, requiredString, "View", useCache, out strArray);

            if (String.IsNullOrEmpty(masterName))
            {
                masterName = "Site";
            }

            string masterPath = this.GetPath(controllerContext, this.MasterLocationFormats,
                    masterName, theme, requiredString, "Master", useCache, out strArray2);

            if (!string.IsNullOrEmpty(viewPath) && (!string.IsNullOrEmpty(masterPath) || string.IsNullOrEmpty(masterName)))
            {
                return new ViewEngineResult(this.CreateView(controllerContext, viewPath, masterPath), this);
            }
            return new ViewEngineResult(strArray.Union(strArray2));
        }

        #endregion
    }
}
