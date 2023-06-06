using P3D.Legacy.Common.Monsters;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace P3D.Legacy.Server.Client.P3D.Extensions
{
    public static class MonsterExtensions
    {
        public static bool IsValidP3D(this IMonsterInstance monster) =>
            (monster.Gender == MonsterGender.Male && monster.StaticData.MaleRatio == 0 ||
             monster.Gender == MonsterGender.Female && Math.Abs(monster.StaticData.MaleRatio - 1) < float.Epsilon ||
             monster.Gender == MonsterGender.Genderless && Math.Abs(monster.StaticData.MaleRatio - (-1)) < float.Epsilon ||
             monster.Gender is MonsterGender.Male or MonsterGender.Female)
            && monster.Ability is not null
            //&& Attacks.All(a => StaticData.LearnableAttacks.Any(la => la.Id == a.StaticData.Id))
            //&& CurrentHP <= EV.HP + IV.HP
            && monster.Level <= 100
            && monster.EV.IsValidEV()
            && monster.IV.IsValidIV();

        public static IDictionary<string, string> ToDictionary(this IMonsterInstance monster)
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

            return dict;
        }

        public static DataItemStorage DictionaryToDataItems(IDictionary<string, string> dict)
        {
            var builder = new StringBuilder();

            foreach (var (key, value) in dict)
                builder.Append("{\"").Append(key).Append('\"').Append(value).Append('}');

            return new DataItemStorage(builder.ToString());
        }

        public static string DictionaryToString(IDictionary<string, string> dict)
        {
            var builder = new StringBuilder();

            foreach (var (key, value) in dict)
                builder.Append("{\"").Append(key).Append('\"').Append(value).Append('}');

            return builder.ToString();
        }
    }
}