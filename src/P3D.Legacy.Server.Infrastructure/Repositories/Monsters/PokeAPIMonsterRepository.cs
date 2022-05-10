using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common.Extensions;
using P3D.Legacy.Common.Monsters;
using P3D.Legacy.Server.Infrastructure.Models.Monsters;
using P3D.Legacy.Server.Infrastructure.Options;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Repositories.Monsters
{
    public class PokeAPIMonsterRepository : IMonsterRepository
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

        public PokeAPIMonsterRepository(ILogger<PokeAPIMonsterRepository> logger, IOptions<PokeAPIOptions> options)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<IMonsterInstance> GetByDataAsync(string monsterDataStr, CancellationToken ct)
        {
            var dict = monsterDataStr.AsSpan().MonsterDataToDictionary();
            var itemId = short.TryParse(dict["Item"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var itemIdVar) ? itemIdVar : -1;

            using var graphQLClient = new GraphQLHttpClient(_options.GraphQLEndpoint, new SystemTextJsonSerializer());
            var monsterStaticDataRequest = new GraphQLRequest
            {
                Query = GetQuery,
                OperationName = "MonsterStaticData",
                Variables = new { ID = dict["Pokemon"], ItemId = itemId }
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
                    _ => throw new NotImplementedException()
                },
                monsterData.Moves.Select(static x => new AttackStaticDataEntity((ushort) x.MoveId, x.Move.Name, (byte) x.Move.PP)).ToList()
            );

            var heldItemData = data.Data.Item.FirstOrDefault();
            var heldItemStaticData = heldItemData is null ? null : new ItemStaticDataEntity(heldItemData.Id, heldItemData.Name);
            var heldItem = heldItemStaticData is null ? null : new ItemEntity(heldItemStaticData);

            var move0 = dict["Attack1"].Split(',');
            var move1 = dict["Attack2"].Split(',');
            var move2 = dict["Attack3"].Split(',');
            var move3 = dict["Attack4"].Split(',');
            var moves = new List<IAttackInstance>();
            if (move0.Length != 1 &&
                ushort.TryParse(move0[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var move0Id) &&
                byte.TryParse(move0[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pp0) &&
                byte.TryParse(move0[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var currentPP0))
            {
                var staticData = new AttackStaticDataEntity(move0Id, "", pp0);
                moves.Add(new AttackEntity(staticData, currentPP0, 0));
            }
            if (move1.Length != 1 &&
                ushort.TryParse(move1[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var move1Id) &&
                byte.TryParse(move1[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pp1) &&
                byte.TryParse(move1[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var currentPP1))
            {
                var staticData = new AttackStaticDataEntity(move1Id, "", pp1);
                moves.Add(new AttackEntity(staticData, currentPP1, 0));
            }
            if (move2.Length != 1 &&
                ushort.TryParse(move2[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var move2Id) &&
                byte.TryParse(move2[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pp2) &&
                byte.TryParse(move2[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var currentPP2))
            {
                var staticData = new AttackStaticDataEntity(move2Id, "", pp2);
                moves.Add(new AttackEntity(staticData, currentPP2, 0));
            }
            if (move3.Length != 1 &&
                ushort.TryParse(move3[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var move3Id) &&
                byte.TryParse(move3[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pp3) &&
                byte.TryParse(move3[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var currentPP3))
            {
                var staticData = new AttackStaticDataEntity(move3Id, "", pp3);
                moves.Add(new AttackEntity(staticData, currentPP3, 0));
            }
            /*
            if (move0.Length != 1 && ushort.TryParse(move0[0], out var move0Id) && monsterData.Moves.FirstOrDefault(x => x.MoveId == move0Id) is var (_, _, (_, pp0)))
            {
                var ppUps = (byte) Math.Round((double) (byte.Parse(move0[1]) - pp0) / pp0 / 0.2D);
                var staticData = monsterStaticData.LearnableAttacks.First(x => x.Id == move0Id);
                moves.Add(new AttackEntity(staticData, byte.Parse(move0[2]), ppUps));
            }
            if (move1.Length != 1 && ushort.TryParse(move1[0], out var move1Id) && monsterData.Moves.FirstOrDefault(x => x.MoveId == move1Id) is var (_, _, (_, pp1)))
            {
                var ppUps = (byte) Math.Round((double) (byte.Parse(move1[1]) - pp1) / pp1 / 0.2D);
                var staticData = monsterStaticData.LearnableAttacks.First(x => x.Id == move1Id);
                moves.Add(new AttackEntity(staticData, byte.Parse(move1[2]), ppUps));
            }
            if (move2.Length != 1 && ushort.TryParse(move2[0], out var move2Id) && monsterData.Moves.FirstOrDefault(x => x.MoveId == move2Id) is var (_, _, (_, pp2)))
            {
                var ppUps = (byte) Math.Round((double) (byte.Parse(move2[1]) - pp2) / pp2 / 0.2D);
                var staticData = monsterStaticData.LearnableAttacks.First(x => x.Id == move2Id);
                moves.Add(new AttackEntity(staticData, byte.Parse(move2[2]), ppUps));
            }
            if (move3.Length != 1 && ushort.TryParse(move3[0], out var move3Id) && monsterData.Moves.FirstOrDefault(x => x.MoveId == move3Id) is var (_, _, (_, pp3)))
            {
                var ppUps = (byte) Math.Round((double) (byte.Parse(move3[1]) - pp3) / pp3 / 0.2D);
                var staticData = monsterStaticData.LearnableAttacks.First(x => x.Id == move3Id);
                moves.Add(new AttackEntity(staticData, byte.Parse(move3[2]), ppUps));
            }
            */

            var monster = new MonsterEntity(monsterDataStr, monsterStaticData, moves, heldItem);
            return monster;
        }

        [SuppressMessage("Performance", "CA1812")]
        private sealed record V2Type(
            [property: JsonPropertyName("id")] int Id,
            [property: JsonPropertyName("name")] string Name
        );

        [SuppressMessage("Performance", "CA1812")]
        private sealed record V2Types(
            [property: JsonPropertyName("pokemon_v2_type")] V2Type Type
        );

        [SuppressMessage("Performance", "CA1812")]
        private sealed record V2Stat(
            [property: JsonPropertyName("name")] string Name
        );

        [SuppressMessage("Performance", "CA1812")]
        private sealed record V2Stats(
            [property: JsonPropertyName("base_stat")] int BaseStat,
            [property: JsonPropertyName("pokemon_v2_stat")] V2Stat Stat
        );

        [SuppressMessage("Performance", "CA1812")]
        private sealed record V2Ability(
            [property: JsonPropertyName("name")] string Name
        );

        [SuppressMessage("Performance", "CA1812")]
        private sealed record V2Abilities(
            [property: JsonPropertyName("pokemon_v2_ability")] V2Ability Ability,
            [property: JsonPropertyName("ability_id")] int AbilityId,
            [property: JsonPropertyName("is_hidden")] bool IsHidden,
            [property: JsonPropertyName("slot")] int Slot
        );

        [SuppressMessage("Performance", "CA1812")]
        private sealed record V2Move(
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("pp")] int PP
        );

        [SuppressMessage("Performance", "CA1812")]
        private sealed record V2Moves(
            [property: JsonPropertyName("move_id")] int MoveId,
            [property: JsonPropertyName("level")] int Level,
            [property: JsonPropertyName("pokemon_v2_move")] V2Move Move
        );

        [SuppressMessage("Performance", "CA1812")]
        private sealed record V2GrowthRate(
            [property: JsonPropertyName("formula")] string Formula,
            [property: JsonPropertyName("name")] string Name
        );

        [SuppressMessage("Performance", "CA1812")]
        private sealed record V2Species(
            [property: JsonPropertyName("gender_rate")] int GenderRate,
            [property: JsonPropertyName("hatch_counter")] int HatchCounter,
            [property: JsonPropertyName("is_baby")] bool IsBaby,
            [property: JsonPropertyName("pokemon_v2_growthrate")] V2GrowthRate GrowthRate
        );

        [SuppressMessage("Performance", "CA1812")]
        private sealed record V2Monster(
            [property: JsonPropertyName("id")] int Id,
            [property: JsonPropertyName("name")] string Name,
            [property: JsonPropertyName("pokemon_v2_pokemontypes")] V2Types[] Types,
            [property: JsonPropertyName("pokemon_v2_pokemonstats")] V2Stats[] Stats,
            [property: JsonPropertyName("pokemon_v2_pokemonabilities")] V2Abilities[] Abilities,
            [property: JsonPropertyName("pokemon_v2_pokemonmoves")] V2Moves[] Moves,
            [property: JsonPropertyName("pokemon_v2_pokemonspecy")] V2Species Species
        );

        [SuppressMessage("Performance", "CA1812")]
        private sealed record V2Item(
            [property: JsonPropertyName("id")] int Id,
            [property: JsonPropertyName("name")] string Name
        );

        [SuppressMessage("Performance", "CA1812")]
        private sealed record Response(
            [property: JsonPropertyName("pokemon_v2_pokemon")] V2Monster[] Monster,
            [property: JsonPropertyName("pokemon_v2_item")] V2Item[] Item
        );
    }
}