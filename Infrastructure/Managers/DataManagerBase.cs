
namespace Infrastructure
{
    public class DataManagerBase
    {
        protected LinguineDbContextFactory _dbf;


         // keep this ABC, but somehow get a way to give it a readonly context factory??
        public DataManagerBase(LinguineDbContextFactory dbf)
        {
            _dbf = dbf;
        }
    }
}
