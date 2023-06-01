using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Polly;

using System;
using System.Threading.Tasks;

namespace P3D.Legacy.Tests.Utils
{
    internal sealed class TestService
    {
        private Action<IServiceCollection> _configureDescriptors = static _ => { };
        private Policy _policy = Policy.NoOp();

        public static TestService CreateNew() => new();

        private TestService() { }

        private void InternalConfigure(IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddLogging(static builder => builder.AddNUnit());
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

        private ServiceProvider CreateAndBuildServiceProvider()
        {
            var serviceDescriptors = new ServiceCollection();
            _configureDescriptors.Invoke(serviceDescriptors);

            InternalConfigure(serviceDescriptors);

            return serviceDescriptors.BuildServiceProvider();
        }

        public void DoTest(Action<IServiceProvider> test) => _policy.Execute(() =>
        {
            using var serviceProvider = CreateAndBuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            test.Invoke(scope.ServiceProvider);
        });

        public Task DoTestAsync(Func<IServiceProvider, Task> test) => _policy.Execute(async () =>
        {
            await using var serviceProvider = CreateAndBuildServiceProvider();
            await using var scope = serviceProvider.CreateAsyncScope();
            await test.Invoke(scope.ServiceProvider);
        });
    }
}