using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Threading;

using Polly;

using System;
using System.Threading.Tasks;

using IAsyncDisposable = System.IAsyncDisposable;

namespace P3D.Legacy.Tests.Utils
{
    internal sealed class TestService : IDisposable, IAsyncDisposable
    {
        private Action<IServiceCollection> _configureDescriptors = _ => { };
        private IServiceProvider _serviceProvider = default!;
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
            _configureDescriptors.Invoke(serviceDescriptors);

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

        public void Dispose()
        {
            using var jctx = new JoinableTaskContext();
            switch (_serviceProvider)
            {
                case IAsyncDisposable asyncDisposable:
                    new JoinableTaskFactory(jctx).Run(() => asyncDisposable.DisposeAsync().AsTask());
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

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
