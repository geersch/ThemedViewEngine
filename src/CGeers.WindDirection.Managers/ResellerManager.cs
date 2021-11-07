using System;
using System.Linq;

namespace CGeers.WindDirection.Managers
{
    public class ResellerManager : Manager
    {
        public string GetThemeForDomain(string domain)
        {
            var q = from r in Context.Resellers
                    where r.Domain == domain
                    select r.Theme;
            string theme = q.SingleOrDefault();
            return !String.IsNullOrEmpty(theme) ? theme : "Default";
        }
    }
}
