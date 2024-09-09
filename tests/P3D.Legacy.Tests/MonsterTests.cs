using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NUnit.Framework;
using NUnit.Framework.Legacy;

using P3D.Legacy.Server.Application.Services;
using P3D.Legacy.Server.Client.P3D.Data.P3DDatas;
using P3D.Legacy.Server.Client.P3D.Extensions;
using P3D.Legacy.Server.Client.P3D.Services;
using P3D.Legacy.Server.Domain.Options;
using P3D.Legacy.Server.Domain.Services;
using P3D.Legacy.Server.Infrastructure;
using P3D.Legacy.Server.Infrastructure.Options;
using P3D.Legacy.Tests.Utils;

using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Tests;

internal sealed class MonsterTests
{
    private static string[] TestCaseSources() => File.ReadAllLines("./Data/Monsters.txt");

    [Test]
    [TestCaseSource(nameof(TestCaseSources))]
    public async Task TestMonsterCreationViaPokeAPIAsync(string line) => await TestService.CreateNew()
        .Configure(static services =>
        {
            var assemblyName = Assembly.GetEntryAssembly()?.GetName();
            var userAgent = $"{assemblyName?.Name ?? "ERROR"} v{assemblyName?.Version?.ToString() ?? "ERROR"}";
            services.AddHttpClient("pokeapi").ConfigureHttpClient((sp, client) =>
            {
                client.BaseAddress = new Uri("https://beta.pokeapi.co/graphql/v1beta");
                client.DefaultRequestHeaders.Add("User-Agent", userAgent);
            }).AddServiceDiscovery();
            services.AddTransient<IMonsterDataProvider, PokeAPIMonsterDataProvider>(sp =>
                {
                    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();

#pragma warning disable IDISP001
                    var httpClient = httpClientFactory.CreateClient("pokeapi");
                    var graphQlClient = new GraphQLHttpClient(new GraphQLHttpClientOptions(), new SystemTextJsonSerializer(), httpClient);
#pragma warning restore IDISP001
                    return ActivatorUtilities.CreateInstance<PokeAPIMonsterDataProvider>(sp, graphQlClient);
                }
            );
            services.AddTransient<IMonsterValidator, DefaultMonsterValidator>();
            services.AddTransient<P3DMonsterConverter>();
        }).DoTestAsync(async sp =>
        {
            var converter = sp.GetRequiredService<P3DMonsterConverter>();
            var validator = sp.GetRequiredService<IMonsterValidator>();
            var monster = await converter.FromP3DStringAsync(line, CancellationToken.None);
            if (await validator.ValidateAsync(monster, CancellationToken.None))
            {
                var convertedBack = await converter.ToP3DStringAsync(monster);
                ClassicAssert.AreEqual(line, convertedBack);
            }
        });

    [Test]
    [TestCaseSource(nameof(TestCaseSources))]
    public void TestMonsterDictionaryTransformation(string line)
    {
        var dict = line.AsSpan().MonsterDataToDictionary();
        var convertedBack = dict.DictionaryToMonsterData();

        ClassicAssert.AreEqual(line, convertedBack);
    }

    [Test]
    public void TestBattleOfferData()
    {
        var monsters = string.Join('|', TestCaseSources());
        var data = new BattleOfferData(monsters);
        var convertedBack = data.ToP3DString();

        ClassicAssert.AreEqual(monsters, convertedBack);
    }

    [Test]
    [TestCaseSource(nameof(TestCaseSources))]
    public void TestTradeData(string line)
    {
        var data = new TradeData(line);
        var convertedBack = data.ToP3DString();

        ClassicAssert.AreEqual(line, convertedBack);
    }
}