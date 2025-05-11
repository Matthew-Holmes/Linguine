
namespace Infrastructure
{
    public class DataManagerBase
    {
        protected LinguineReadonlyDbContextFactory _dbf;


         // keep this ABC, but somehow get a way to give it a readonly context factory??
        public DataManagerBase(LinguineReadonlyDbContextFactory dbf)
        {
            _dbf = dbf;
        }
    }
}
