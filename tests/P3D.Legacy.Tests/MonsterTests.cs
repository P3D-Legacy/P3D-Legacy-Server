using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using P3D.Legacy.Common.Data.P3DDatas;
using P3D.Legacy.Common.Extensions;
using P3D.Legacy.Server.Infrastructure.Options;
using P3D.Legacy.Server.Infrastructure.Repositories.Monsters;
using P3D.Legacy.Tests.Utils;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Tests
{
    internal sealed class MonsterTests
    {
        private static string[] TestCaseSources() => File.ReadAllLines("./Data/Monsters.txt");

        [Test]
        [TestCaseSource(nameof(TestCaseSources))]
        public async Task TestMonsterCreationViaPokeAPIAsync(string line) => await TestService.CreateNew()
            .Configure(static services =>
            {
                services.AddTransient<IOptions<PokeAPIOptions>>(static _ => new OptionsWrapper<PokeAPIOptions>(new PokeAPIOptions
                {
                    GraphQLEndpoint = "https://beta.pokeapi.co/graphql/v1beta"
                }));
                services.AddTransient<PokeAPIMonsterRepository>();
            }).DoTestAsync(async sp =>
            {
                var repository = sp.GetRequiredService<PokeAPIMonsterRepository>();
                var monster = await repository.GetByDataAsync(line, CancellationToken.None);
                if (monster.IsValidP3D())
                {
                    var convertedBack = MonsterExtensions.DictionaryToString(monster.ToDictionary());
                    Assert.AreEqual(line, convertedBack);
                }
            });

        [Test]
        [TestCaseSource(nameof(TestCaseSources))]
        public void TestMonsterDictionaryTransformation(string line)
        {
            var dict = line.AsSpan().MonsterDataToDictionary();
            var convertedBack = dict.DictionaryToMonsterData();

            Assert.AreEqual(line, convertedBack);
        }

        [Test]
        public void TestBattleOfferData()
        {
            var monsters = string.Join('|', TestCaseSources());
            var data = new BattleOfferData(monsters);
            var convertedBack = data.ToP3DString();

            Assert.AreEqual(monsters, convertedBack);
        }

        [Test]
        [TestCaseSource(nameof(TestCaseSources))]
        public void TestTradeData(string line)
        {
            var data = new TradeData(line);
            var convertedBack = data.ToP3DString();

            Assert.AreEqual(line, convertedBack);
        }
    }
}