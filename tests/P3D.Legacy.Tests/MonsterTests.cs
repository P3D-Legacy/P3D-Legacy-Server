using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Data.P3DData;
using P3D.Legacy.Common.Extensions;
using P3D.Legacy.Server.Infrastructure.Options;
using P3D.Legacy.Server.Infrastructure.Repositories.Monsters;
using P3D.Legacy.Tests.Utils;

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace P3D.Legacy.Tests
{
    public class MonsterTests
    {
        private static string[] TestCaseSources() => File.ReadAllLines("./Data/Monsters.txt");

        [Test]
        [TestCaseSource(nameof(TestCaseSources))]
        public async Task TestMonsterCreationViaPokeAPIAsync(string line)
        {
            await using var testService = TestService.CreateNew()
                .Configure(services =>
                {
                    services.AddTransient<IOptions<PokeAPIOptions>>(_ => new OptionsWrapper<PokeAPIOptions>(new PokeAPIOptions()
                    {
                        GraphQLEndpoint = "https://beta.pokeapi.co/graphql/v1beta"
                    }));
                    services.AddTransient<PokeAPIMonsterRepository>();
                });

            await testService.DoTestAsync(async serviceProvider =>
            {
                var repository = serviceProvider.GetRequiredService<PokeAPIMonsterRepository>();
                var monster = await repository.GetByDataAsync(line);
                if (monster.IsValidP3D())
                {
                    var convertedBack = MonsterExtensions.DictionaryToString(monster.ToDictionary());
                    Assert.AreEqual(line, convertedBack);
                }
            });
        }

        [Test]
        [TestCaseSource(nameof(TestCaseSources))]
        public void TestMonsterDictionaryTransformation(string line)
        {
            using var testService = TestService.CreateNew();

            testService.DoTest(serviceProvider =>
            {
                var dict = line.AsSpan().MonsterDataToDictionary();
                var convertedBack = dict.DictionaryToMonsterData();
                Assert.AreEqual(line, convertedBack);
            });
        }

        [Test]
        public void TestBattleOfferData()
        {
            using var testService = TestService.CreateNew();

            testService.DoTest(serviceProvider =>
            {
                var monsters = string.Join('|', TestCaseSources());
                var data = new BattleOfferData(monsters);
                var convertedBack = data.ToP3DString();

                Assert.AreEqual(monsters, convertedBack);
            });
        }

        [Test]
        [TestCaseSource(nameof(TestCaseSources))]
        public void TestTradeData(string line)
        {
            using var testService = TestService.CreateNew();

            testService.DoTest(serviceProvider =>
            {
                var data = new TradeData(line);
                var convertedBack = data.ToP3DString();

                Assert.AreEqual(line, convertedBack);
            });
        }
    }
}