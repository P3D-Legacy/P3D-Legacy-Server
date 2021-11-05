using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Polly;

using System;
using System.Threading.Tasks;

namespace P3D.Legacy.Tests.Utils
{
    public sealed class TestService : IDisposable, IAsyncDisposable
    {
        private Action<IServiceCollection> _configureDescriptors;
        private IServiceProvider _serviceProvider;
        private Policy _policy = Policy.NoOp();

        public static TestService CreateNew() => new();

        private TestService() { }

        private void InternalConfigure(IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddLogging(builder => builder.AddNUnit());
        }

        public TestService Configure(Action<IServiceCollection> services)
        {
            _configureDescriptors += services;
            return this;
        }

        public TestService AddPolicy(Policy policy)
        {
            _policy = _policy.Wrap(policy);
            return this;
        }

        public TestService AddTimeout(TimeSpan timeout)
        {
            _policy = _policy.Wrap(Policy.Timeout(timeout, Polly.Timeout.TimeoutStrategy.Pessimistic));
            return this;
        }

        private IServiceProvider CreateAndBuildServiceProvider()
        {
            var serviceDescriptors = new ServiceCollection();
            _configureDescriptors?.Invoke(serviceDescriptors);

            InternalConfigure(serviceDescriptors);

            return serviceDescriptors.BuildServiceProvider();
        }

        public void DoTest(Action<IServiceProvider> test)
        {
            _serviceProvider = CreateAndBuildServiceProvider();

            _policy.Execute(() =>
            {
                using var scope = _serviceProvider.CreateScope();
                test.Invoke(scope.ServiceProvider);
            });
        }

        public async Task DoTestAsync(Func<IServiceProvider, Task> test)
        {
            _serviceProvider = CreateAndBuildServiceProvider();

            await _policy.Execute(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                await test.Invoke(scope.ServiceProvider);
            });
        }

#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
        public void Dispose() => DisposeAsync().AsTask().GetAwaiter().GetResult();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits

        public async ValueTask DisposeAsync()
        {
            switch (_serviceProvider)
            {
                case IAsyncDisposable asyncDisposable:
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }
    }
}
