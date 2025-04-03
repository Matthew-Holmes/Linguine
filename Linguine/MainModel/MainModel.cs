using System;
using System.IO;
using System.Threading.Tasks;
using Infrastructure;
using Config;

namespace Linguine
{
    public partial class MainModel : IDisposable
    {
        public event EventHandler? Loaded;
        public event EventHandler? LoadingFailed;

        private LinguineDbContextFactory _linguineDbContextFactory;
        public LinguineDbContextFactory LinguineFactory { get => _linguineDbContextFactory; }
        

        public MainModel() { }

        public void BeginLoading()
        {
            Task.Run(() => Load()); // load in background
        }

        private void Load()
        {
            try
            {
                Config.Config config = ConfigManager.Config;

                if (config.DatabaseDirectory != null)
                {
                    Directory.CreateDirectory(config.DatabaseDirectory);
                }

                _linguineDbContextFactory = new LinguineDbContextFactory(config.GetDatabaseString());
                var context = LinguineFactory.CreateDbContext();
                context.Database.EnsureCreated();
                context.Dispose();

                LoadManagers();
                LoadServices();

                Loaded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                LoadingFailed?.Invoke(this, EventArgs.Empty);
            }
        }



        public void Dispose()
        {
            // clean up our events
            foreach (Delegate d in LoadingFailed.GetInvocationList())
            {
                LoadingFailed -= (EventHandler)d;
            }
            foreach (Delegate d in Loaded.GetInvocationList())
            {
                Loaded -= (EventHandler)d;
            }
            foreach (Delegate d in SessionsChanged.GetInvocationList())
            {
                SessionsChanged -= (EventHandler)d;
            }
        }
    }
}
