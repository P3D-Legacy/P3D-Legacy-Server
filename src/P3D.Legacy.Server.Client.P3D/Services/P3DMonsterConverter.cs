using P3D.Legacy.Common.Monsters;
using P3D.Legacy.Server.Client.P3D.Extensions;
using P3D.Legacy.Server.Infrastructure;
using P3D.Legacy.Server.Infrastructure.Models.Monsters;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Client.P3D.Services;

public sealed class P3DMonsterConverter
{
    private readonly IMonsterDataProvider _monsterDataProvider;

    public P3DMonsterConverter(IMonsterDataProvider monsterDataProvider)
    {
        _monsterDataProvider = monsterDataProvider;
    }

    public async Task<IMonsterInstance> FromP3DStringAsync(string monsterDataStr, CancellationToken ct)
    {
        var dict = monsterDataStr.AsSpan().MonsterDataToDictionary();
        var id = int.Parse(dict["Pokemon"], CultureInfo.InvariantCulture);
        var itemId = short.TryParse(dict["Item"], NumberStyles.Integer, CultureInfo.InvariantCulture, out var itemIdVar) ? itemIdVar : -1;

        var (monsterStaticData, helItem) = await _monsterDataProvider.GetStaticDataAsync(id, itemId, ct);

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
        //if (move0.Length != 1 && ushort.TryParse(move0[0], out var move0Id) && monsterData.Moves.FirstOrDefault(x => x.MoveId == move0Id) is var (_, _, (_, pp0)))
        //{
        //    var ppUps = (byte) Math.Round((double) (byte.Parse(move0[1]) - pp0) / pp0 / 0.2D);
        //    var staticData = monsterStaticData.LearnableAttacks.First(x => x.Id == move0Id);
        //    moves.Add(new AttackEntity(staticData, byte.Parse(move0[2]), ppUps));
        //}
        //if (move1.Length != 1 && ushort.TryParse(move1[0], out var move1Id) && monsterData.Moves.FirstOrDefault(x => x.MoveId == move1Id) is var (_, _, (_, pp1)))
        //{
        //    var ppUps = (byte) Math.Round((double) (byte.Parse(move1[1]) - pp1) / pp1 / 0.2D);
        //    var staticData = monsterStaticData.LearnableAttacks.First(x => x.Id == move1Id);
        //    moves.Add(new AttackEntity(staticData, byte.Parse(move1[2]), ppUps));
        //}
        //if (move2.Length != 1 && ushort.TryParse(move2[0], out var move2Id) && monsterData.Moves.FirstOrDefault(x => x.MoveId == move2Id) is var (_, _, (_, pp2)))
        //{
        //    var ppUps = (byte) Math.Round((double) (byte.Parse(move2[1]) - pp2) / pp2 / 0.2D);
        //    var staticData = monsterStaticData.LearnableAttacks.First(x => x.Id == move2Id);
        //    moves.Add(new AttackEntity(staticData, byte.Parse(move2[2]), ppUps));
        //}
        //if (move3.Length != 1 && ushort.TryParse(move3[0], out var move3Id) && monsterData.Moves.FirstOrDefault(x => x.MoveId == move3Id) is var (_, _, (_, pp3)))
        //{
        //    var ppUps = (byte) Math.Round((double) (byte.Parse(move3[1]) - pp3) / pp3 / 0.2D);
        //    var staticData = monsterStaticData.LearnableAttacks.First(x => x.Id == move3Id);
        //    moves.Add(new AttackEntity(staticData, byte.Parse(move3[2]), ppUps));
        //}

        return new P3DMonsterEntity(monsterDataStr, monsterStaticData, moves, helItem);
    }

    public Task<string> ToP3DStringAsync(IMonsterInstance monster)
    {
        var dict = new Dictionary<string, string>(StringComparer.Ordinal);
        dict.Add("Pokemon", $"[{monster.StaticData.Id}]");
        dict.Add("OriginalNumber", $"[{monster.Metadata["OriginalNumber"]}]");
        dict.Add("Experience", $"[{monster.Experience}]");
        dict.Add("Gender", $"[{monster.Gender switch { MonsterGender.Male => 0, MonsterGender.Female => 1, MonsterGender.Genderless => 2, _ => throw new ArgumentOutOfRangeException(nameof(monster), nameof(monster.Gender)) }}]");
        dict.Add("EggSteps", $"[{monster.EggSteps}]");
        dict.Add("Item", $"[{monster.HeldItem?.StaticData.Id ?? 0}]");
        dict.Add("ItemData", $"[{monster.Metadata["ItemData"]}]");
        dict.Add("NickName", $"[{monster.CatchInfo.Nickname ?? string.Empty}]");
        dict.Add("Level", $"[{monster.Level}]");
        dict.Add("OT", $"[{(monster.CatchInfo.TrainerId.HasValue ? monster.CatchInfo.TrainerId.Value.ToString(CultureInfo.InvariantCulture) : string.Empty)}]");
        dict.Add("Ability", $"[{monster.Ability.StaticData.Id}]");
        dict.Add("Status", $"[{monster.Metadata["Status"]}]");
        dict.Add("Nature", $"[{monster.Nature}]");
        dict.Add("CatchLocation", $"[{monster.CatchInfo.Location}]");
        dict.Add("CatchTrainer", $"[{monster.CatchInfo.TrainerName}]");
        dict.Add("CatchBall", $"[{monster.CatchInfo.ContainerId}]");
        dict.Add("CatchMethod", $"[{monster.CatchInfo.Method}]");
        dict.Add("Friendship", $"[{monster.Friendship}]");
        dict.Add("isShiny", $"[{(monster.IsShiny ? 1 : 0)}]");

        if (monster.Attacks.Count > 0)
        {
            var attack = monster.Attacks[0];
            var pp = attack.StaticData.PP;
            var ppMax = (int) (attack.PPUps > 0 ? pp + (pp * 0.2D * attack.PPUps) : pp);
            dict.Add("Attack1", $"[{attack.StaticData.Id},{ppMax},{attack.PPCurrent}]");
        }
        else
        {
            dict.Add("Attack1", "[]");
        }
        if (monster.Attacks.Count > 1)
        {
            var attack = monster.Attacks[1];
            var pp = attack.StaticData.PP;
            var ppMax = (int) (attack.PPUps > 0 ? pp + (pp * 0.2D * attack.PPUps) : pp);
            dict.Add("Attack2", $"[{attack.StaticData.Id},{ppMax},{attack.PPCurrent}]");
        }
        else
        {
            dict.Add("Attack2", "[]");
        }
        if (monster.Attacks.Count > 2)
        {
            var attack = monster.Attacks[2];
            var pp = attack.StaticData.PP;
            var ppMax = (int) (attack.PPUps > 0 ? pp + (pp * 0.2D * attack.PPUps) : pp);
            dict.Add("Attack3", $"[{attack.StaticData.Id},{ppMax},{attack.PPCurrent}]");
        }
        else
        {
            dict.Add("Attack3", "[]");
        }
        if (monster.Attacks.Count > 3)
        {
            var attack = monster.Attacks[3];
            var pp = attack.StaticData.PP;
            var ppMax = (int) (attack.PPUps > 0 ? pp + (pp * 0.2D * attack.PPUps) : pp);
            dict.Add("Attack4", $"[{attack.StaticData.Id},{ppMax},{attack.PPCurrent}]");
        }
        else
        {
            dict.Add("Attack4", "[]");
        }

        dict.Add("HP", $"[{monster.CurrentHP}]");
        dict.Add("EVs", $"[{monster.EV.HP},{monster.EV.Attack},{monster.EV.Defense},{monster.EV.SpecialAttack},{monster.EV.SpecialDefense},{monster.EV.Speed}]");
        dict.Add("IVs", $"[{monster.IV.HP},{monster.IV.Attack},{monster.IV.Defense},{monster.IV.SpecialAttack},{monster.IV.SpecialDefense},{monster.IV.Speed}]");
        dict.Add("AdditionalData", $"[{monster.Metadata["AdditionalData"]}]");
        dict.Add("IDValue", $"[{monster.Metadata["IDValue"]}]");

        return Task.FromResult(dict.DictionaryToMonsterData());
    }
}