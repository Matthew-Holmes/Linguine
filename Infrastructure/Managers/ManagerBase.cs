
namespace Infrastructure
{ 
    public class ManagerBase
    {
        protected LinguineReadonlyDbContextFactory _dbf;

        public ManagerBase(LinguineReadonlyDbContextFactory dbf)
        {
            _dbf = dbf;
        }
    }
}
