using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine.Sketches
{
    // some ideas for when the manual state/dependency checking becomes a handful

    public abstract class ManagedService
    {
        public ServiceState State { get; protected set; } = ServiceState.NotStarted;

        public async Task StartAsync()
        {
            if (State != ServiceState.NotStarted)
                throw new InvalidOperationException("Service already started or in invalid state.");

            State = ServiceState.Loading;

            try
            {
                await OnStartAsync();
                State = ServiceState.Loaded;
            }
            catch (Exception)
            {
                State = ServiceState.Failed;
                throw;
            }
        }

        protected abstract Task OnStartAsync();
    }

    public interface IDependentService
    {
        List<string> Dependencies { get; }
    }

    public class EngineStateNotifier
    {
        private TaskCompletionSource<bool> _tcs = new();

        public void NotifyBuilt()
        {
            _tcs.TrySetResult(true);
        }

        public Task WaitUntilBuiltAsync() => _tcs.Task;
    }


    public class ServiceRegistry
    {
        private readonly Dictionary<Type, ManagedService> _services = new();

        public void Register<T>(T service) where T : ManagedService
            => _services[typeof(T)] = service;

        public T Get<T>() where T : ManagedService
            => (T)_services[typeof(T)];

        public async Task StartAllAsync()
        {
            foreach (var service in _services.Values)
            {
                if (service.State == ServiceState.NotStarted)
                    await service.StartAsync();
            }
        }
    }


}
