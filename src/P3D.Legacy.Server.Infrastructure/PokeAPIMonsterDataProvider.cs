using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common.Monsters;
using P3D.Legacy.Server.Infrastructure.Models.Monsters;
using P3D.Legacy.Server.Infrastructure.Options;

using System;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure
{
    public class PokeAPIMonsterDataProvider : IMonsterDataProvider
    {
        private static readonly string GetQuery = @"
query MonsterStaticData($id: Int, $itemId: Int) {
  pokemon_v2_pokemon(where: {id: {_eq: $id}}) {
    id
    name

    pokemon_v2_pokemontypes {
      pokemon_v2_type {
        name
        id
      }
    }

    pokemon_v2_pokemonstats {
      base_stat
      pokemon_v2_stat {
        name
      }
    }

    pokemon_v2_pokemonabilities {
      pokemon_v2_ability {
        name
      }
      ability_id
      is_hidden
      slot
    }

    pokemon_v2_pokemonmoves {
      move_id
      level
      pokemon_v2_move {
        name
        pp
      }
    }

    pokemon_v2_pokemonspecy {
      gender_rate
      hatch_counter
      is_baby
      pokemon_v2_growthrate {
        formula
        name
      }
    }
  }
  pokemon_v2_item(where: {id: {_eq: $itemId}}) {
    id
    name
  }
}
";

        private readonly ILogger _logger;
        private readonly PokeAPIOptions _options;

        public PokeAPIMonsterDataProvider(ILogger<PokeAPIMonsterDataProvider> logger, IOptions<PokeAPIOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc />
        public async Task<(IMonsterStaticData, IItemInstance?)> GetStaticDataAsync(int id, int itemId, CancellationToken ct)
        {
            using var graphQLClient = new GraphQLHttpClient(_options.GraphQLEndpoint, new SystemTextJsonSerializer());
            var monsterStaticDataRequest = new GraphQLRequest
            {
                Query = GetQuery,
                OperationName = "MonsterStaticData",
                Variables = new { ID = id, ItemId = itemId }
            };
            var data = await graphQLClient.SendQueryAsync<Response>(monsterStaticDataRequest, ct);


            var monsterData = data.Data.Monster[0];
            var monsterStaticData = new MonsterStaticDataEntity(
                (short) monsterData.Id,
                monsterData.Name,
                Stats.None,
                0,
                new AbilityContainer(monsterData.Abilities.Select(static x => new AbilityEntity(new AbilityStaticDataEntity((short) x.AbilityId, x.Ability.Name), x.IsHidden)).Cast<IAbilityInstance>().ToArray()),
                monsterData.Species.GenderRate != -1 ? monsterData.Species.GenderRate * 1f / 8f : -1,
                monsterData.Species.HatchCounter,
                monsterData.Species.IsBaby,
                monsterData.Species.GrowthRate.Name switch
                {
                    "slow" => ExperienceType.Slow,
                    "medium" => ExperienceType.MediumFast,
                    "fast" => ExperienceType.Fast,
                    "medium-slow" => ExperienceType.MediumSlow,
                    "slow-then-very-fast" => ExperienceType.Erratic,
                    "fast-then-very-slow" => ExperienceType.Fluctuating,
                    _ => throw new NotSupportedException()
                },
                monsterData.Moves.Select(static x => new AttackStaticDataEntity((ushort) x.MoveId, x.Move.Name, (byte) x.Move.PP)).ToList()
            );

            var heldItemData = data.Data.Item.FirstOrDefault();
            var heldItemStaticData = heldItemData is null ? null : new ItemStaticDataEntity(heldItemData.Id, heldItemData.Name);
            var heldItem = heldItemStaticData is null ? null : new ItemEntity(heldItemStaticData);

            return (monsterStaticData, heldItem);
        }

        private sealed record V2Type(
            [property: JsonPropertyName("id")] int Id,
            [property: JsonPropertyName("name")] string Name
        );

        private sealed record V2Types(
            [property: JsonPropertyName("pokemon_v2_type")] V2Type Type
        );

        private sealed record V2Stat(
            [property: JsonPropertyName("name")] string Name
        );

        private sealed record V2Stats(
            [property: JsonPropertyName("base_stat")] int BaseStat,
            [property: JsonPropertyName("pokemon_v2_stat")] V2Stat Stat
        );

        private sealed record V2Ability(
            [property: JsonPropertyName("name")] string Name
        );

        private sealed record V2Abilities(
            [property: JsonPropertyName("pokemon_v2_ability")] V2Ability Ability,
            [property: JsonPropertyName("ability_id")] int AbilityId,
            [property: JsonPropertyName("is_hidden")] bool IsHidden,
            [property: JsonPropertyName("slot")] int Slot
        );

        private sealed record V2Move(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("pp")] int PP
        );

        private sealed record V2Moves(
            [property: JsonPropertyName("move_id")] int MoveId,
            [property: JsonPropertyName("level")] int Level,
            [property: JsonPropertyName("pokemon_v2_move")] V2Move Move
        );

        private sealed record V2GrowthRate(
            [property: JsonPropertyName("formula")] string Formula,
            [property: JsonPropertyName("name")] string Name
        );

        private sealed record V2Species(
            [property: JsonPropertyName("gender_rate")] int GenderRate,
            [property: JsonPropertyName("hatch_counter")] int HatchCounter,
            [property: JsonPropertyName("is_baby")] bool IsBaby,
            [property: JsonPropertyName("pokemon_v2_growthrate")] V2GrowthRate GrowthRate
        );

        private sealed record V2Monster(
            [property: JsonPropertyName("id")] int Id,
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("pokemon_v2_pokemontypes")] V2Types[] Types,
            [property: JsonPropertyName("pokemon_v2_pokemonstats")] V2Stats[] Stats,
            [property: JsonPropertyName("pokemon_v2_pokemonabilities")] V2Abilities[] Abilities,
            [property: JsonPropertyName("pokemon_v2_pokemonmoves")] V2Moves[] Moves,
            [property: JsonPropertyName("pokemon_v2_pokemonspecy")] V2Species Species
        );

        private sealed record V2Item(
            [property: JsonPropertyName("id")] int Id,
            [property: JsonPropertyName("name")] string Name
        );

        private sealed record Response(
            [property: JsonPropertyName("pokemon_v2_pokemon")] V2Monster[] Monster,
            [property: JsonPropertyName("pokemon_v2_item")] V2Item[] Item
        );
    }
}