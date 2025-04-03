
namespace Infrastructure
{ 
    public class ManagerBase
    {
        protected LinguineDbContextFactory _dbf;

        public ManagerBase(LinguineDbContextFactory dbf)
        {
            _dbf = dbf;
        }
    }
}
