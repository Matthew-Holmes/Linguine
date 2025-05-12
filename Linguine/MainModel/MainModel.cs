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

        private LinguineDbContextFactory         _linguineDbContextFactory;
        private LinguineReadonlyDbContextFactory _linguineReadonlyDbContextFactory;


        public LinguineDbContextFactory         LinguineFactory         { get => _linguineDbContextFactory; }
        public LinguineReadonlyDbContextFactory ReadonlyLinguineFactory { get => _linguineReadonlyDbContextFactory; }

        internal ServiceManager SM { get; set; }
        

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

                StartContextFactories(config);

                SM = new ServiceManager(ReadonlyLinguineFactory);

                EnsureDatabase();

                SM.Initialise();

                Loaded?.Invoke(this, EventArgs.Empty);

            }
            catch (Exception e)
            {
                LoadingFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void StartContextFactories(Config.Config config)
        {
            String connString = config.GetDatabaseString();
            _linguineDbContextFactory = new LinguineDbContextFactory(connString);
            _linguineReadonlyDbContextFactory = new LinguineReadonlyDbContextFactory(_linguineDbContextFactory);
        }

        private void EnsureDatabase()
        {
            var context = LinguineFactory.CreateDbContext();
            context.Database.EnsureCreated();
            context.Dispose();
        }

        public void Dispose()
        {
            // stop any running jobs
            foreach (var kvp in _bulkCancellations)
            {
                kvp.Value.Cancel();
            }

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
