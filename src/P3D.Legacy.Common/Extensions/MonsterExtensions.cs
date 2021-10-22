using P3D.Legacy.Common.Monsters;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace P3D.Legacy.Common.Extensions
{
    public static class MonsterExtensions
    {
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public static bool IsValidP3D(this IMonsterInstance monster) =>
            (monster.Gender == MonsterGender.Male && monster.StaticData.MaleRatio == 0 ||
             monster.Gender == MonsterGender.Female && monster.StaticData.MaleRatio == 1 ||
             monster.Gender == MonsterGender.Genderless && monster.StaticData.MaleRatio == -1 ||
             monster.Gender is MonsterGender.Male or MonsterGender.Female)
            && monster.Ability is not null
            //&& Attacks.All(a => StaticData.LearnableAttacks.Any(la => la.Id == a.StaticData.Id))
            //&& CurrentHP <= EV.HP + IV.HP
            && monster.Level <= 105 // We keep a margin here just for the sake
            && monster.EV.IsValidEV()
            && monster.IV.IsValidIV();

        public static Dictionary<string, string> ToDictionary(this IMonsterInstance monster)
        {
#pragma warning disable IDE0028 // Simplify collection initialization
            // ReSharper disable once UseObjectOrCollectionInitializer
            var dict = new Dictionary<string, string>();
            dict.Add("Pokemon", $"[{monster.StaticData.Id}]");
            dict.Add("OriginalNumber", $"[{monster.Metadata["OriginalNumber"]}]");
            dict.Add("Experience", $"[{monster.Experience}]");
            dict.Add("Gender", $"[{monster.Gender switch { MonsterGender.Male => 0, MonsterGender.Female => 1, MonsterGender.Genderless => 2, _ => throw new ArgumentOutOfRangeException() }}]");
            dict.Add("EggSteps", $"[{monster.EggSteps}]");
            dict.Add("Item", $"[{monster.HeldItem?.StaticData.Id ?? 0}]");
            dict.Add("ItemData", $"[{monster.Metadata["ItemData"]}]");
            dict.Add("NickName", $"[{monster.CatchInfo.Nickname ?? string.Empty}]");
            dict.Add("Level", $"[{monster.Level}]");
            dict.Add("OT", $"[{(monster.CatchInfo.TrainerId.HasValue ? monster.CatchInfo.TrainerId : string.Empty)}]");
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
                dict.Add("Attack1", "[]");
            if (monster.Attacks.Count > 1)
            {
                var attack = monster.Attacks[1];
                var pp = attack.StaticData.PP;
                var ppMax = (int) (attack.PPUps > 0 ? pp + (pp * 0.2D * attack.PPUps) : pp);
                dict.Add("Attack2", $"[{attack.StaticData.Id},{ppMax},{attack.PPCurrent}]");
            }
            else
                dict.Add("Attack2", "[]");
            if (monster.Attacks.Count > 2)
            {
                var attack = monster.Attacks[2];
                var pp = attack.StaticData.PP;
                var ppMax = (int) (attack.PPUps > 0 ? pp + (pp * 0.2D * attack.PPUps) : pp);
                dict.Add("Attack3", $"[{attack.StaticData.Id},{ppMax},{attack.PPCurrent}]");
            }
            else
                dict.Add("Attack3", "[]");
            if (monster.Attacks.Count > 3)
            {
                var attack = monster.Attacks[3];
                var pp = attack.StaticData.PP;
                var ppMax =  (int) (attack.PPUps > 0 ? pp + (pp * 0.2D * attack.PPUps) : pp);
                dict.Add("Attack4", $"[{attack.StaticData.Id},{ppMax},{attack.PPCurrent}]");
            }
            else
                dict.Add("Attack4", "[]");

            dict.Add("HP", $"[{monster.CurrentHP}]");
            dict.Add("EVs", $"[{monster.EV.HP},{monster.EV.Attack},{monster.EV.Defense},{monster.EV.SpecialAttack},{monster.EV.SpecialDefense},{monster.EV.Speed}]");
            dict.Add("IVs", $"[{monster.IV.HP},{monster.IV.Attack},{monster.IV.Defense},{monster.IV.SpecialAttack},{monster.IV.SpecialDefense},{monster.IV.Speed}]");
            dict.Add("AdditionalData", $"[{monster.Metadata["AdditionalData"]}]");
            dict.Add("IDValue", $"[{monster.Metadata["IDValue"]}]");
#pragma warning restore IDE0028 // Simplify collection initialization

            return dict;
        }

        public static DataItemStorage DictionaryToDataItems(Dictionary<string, string> dict)
        {
            var builder = new StringBuilder();

            foreach (var (key, value) in dict)
                builder.Append("{\"").Append(key).Append('\"').Append(value).Append('}');

            return new DataItemStorage(builder.ToString());
        }

        public static string DictionaryToString(Dictionary<string, string> dict)
        {
            var builder = new StringBuilder();

            foreach (var (key, value) in dict)
                builder.Append("{\"").Append(key).Append('\"').Append(value).Append('}');

            return builder.ToString();
        }
    }
}