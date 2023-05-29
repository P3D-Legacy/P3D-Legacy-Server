using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Server.Benchmark.Options;

using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Benchmark.Services
{
    public class RunBenchmarkCommand : RootCommand
    {
        public RunBenchmarkCommand() : base("Generates ThirdPartyNotices")
        {
            AddOption(new Option<string>("--benchmark-type", static () => "status", "Benchmark Type (status, client)"));
            AddOption(new Option<string>("--host", static () => "localhost", "Server Host"));
            AddOption(new Option<ushort>("--port", static () => 15124, "Server Port"));
            AddOption(new Option<int>("--batch", static () => 100, "Batch count"));
        }

        internal new sealed class Handler : ICommandHandler
        {
            private readonly ILogger _logger;
            private readonly CLIOptions _options;
            private readonly BenchmarkService _benchmarkService;
            private readonly BenchmarkStatusService _benchmarkStatusService;

            public Handler(ILogger<Handler> logger, IOptions<CLIOptions> options, BenchmarkService benchmarkService, BenchmarkStatusService benchmarkStatusService)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
                _options = options.Value ?? throw new ArgumentNullException(nameof(options));
                _benchmarkService = benchmarkService ?? throw new ArgumentNullException(nameof(benchmarkService));
                _benchmarkStatusService = benchmarkStatusService ?? throw new ArgumentNullException(nameof(benchmarkStatusService));
            }

            public int Invoke(InvocationContext context) => InvokeAsync(context).GetAwaiter().GetResult();

            public async Task<int> InvokeAsync(InvocationContext context)
            {
                switch (_options.BenchmarkType)
                {
                    case "status":
                        await _benchmarkStatusService.ExecuteAsync(context.GetCancellationToken());
                        break;
                    case "client":
                        await _benchmarkService.ExecuteAsync(context.GetCancellationToken());
                        break;
                    default:
                        return 1;
                }

                return 0;
            }
        }
    }
}