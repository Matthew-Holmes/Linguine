
namespace Infrastructure
{
    // TODO - Template these instead of ABC??
    public class DataManagerBase
    {
        protected LinguineDbContextFactory _dbf;

        public DataManagerBase(LinguineDbContextFactory dbf)
        {
            _dbf = dbf;
        }
    }
}
