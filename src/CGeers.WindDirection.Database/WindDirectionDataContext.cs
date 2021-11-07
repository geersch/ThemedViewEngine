using System;
using System.Configuration;

namespace CGeers.WindDirection.Database
{
    public partial class WindDirectionDataContext
    {
        private static readonly string ConnectionString;

        static WindDirectionDataContext()
        {
            ConnectionStringSettings settings = ConfigurationManager.ConnectionStrings["WindDirection"];
            ConnectionString = settings != null ? settings.ConnectionString : String.Empty;
        }

        public WindDirectionDataContext() : base(ConnectionString) { }
    }
}