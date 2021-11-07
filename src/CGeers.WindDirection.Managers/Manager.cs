using CGeers.WindDirection.Database;

namespace CGeers.WindDirection.Managers
{
    public abstract class Manager
    {
        protected Manager()
        {
            Context = new WindDirectionDataContext();
        }

        public WindDirectionDataContext Context { get; set; }
    }
}