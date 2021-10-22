using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using NUnit.Framework;

using P3D.Legacy.Common;
using P3D.Legacy.Common.Extensions;
using P3D.Legacy.Server.Infrastructure.Monsters;
using P3D.Legacy.Server.Infrastructure.Options;
using P3D.Legacy.Tests.Utils;

using System.IO;
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

            await testService.DoTest(async serviceProvider =>
            {
                var repository = serviceProvider.GetRequiredService<PokeAPIMonsterRepository>();
                var monster = await repository.GetByDataAsync(new DataItemStorage(line));
                if (monster.IsValidP3D())
                {
                    var convertedBack = MonsterExtensions.DictionaryToString(monster.ToDictionary());
                    Assert.AreEqual(line, convertedBack);
                }
            });
        }
    }
}