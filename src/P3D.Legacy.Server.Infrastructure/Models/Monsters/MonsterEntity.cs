using P3D.Legacy.Common.Extensions;
using P3D.Legacy.Common.Monsters;

using System;
using System.Collections.Generic;
using System.Globalization;

namespace P3D.Legacy.Server.Infrastructure.Models.Monsters
{
    public sealed class MonsterEntity : IMonsterInstance
    {
        public IMonsterStaticData StaticData { get; }
        public CatchInfo CatchInfo { get; }
        public MonsterGender Gender { get; }
        public IAbilityInstance Ability { get; }
        public byte Nature { get; }
        public long Experience { get; }
        public byte Level => ExperienceCalculator.LevelForExperienceValue(StaticData.ExperienceType, Experience);
        public Stats EV { get; }
        public Stats IV { get; }
        public short CurrentHP { get; }
        public byte StatusEffect { get; }
        public byte Friendship { get; }
        public bool IsShiny { get; }
        public int EggSteps { get; }
        public IReadOnlyList<IAttackInstance> Attacks { get; }
        public IItemInstance? HeldItem { get; }
        public IDictionary<string, string> Metadata { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

        public MonsterEntity(in ReadOnlySpan<char> monsterData, IMonsterStaticData monsterStaticData, IReadOnlyList<IAttackInstance> attacks, IItemInstance? heldItem)
        {
            StaticData = monsterStaticData;

            var dict = monsterData.MonsterDataToDictionary();

            Gender = int.Parse(dict["Gender"], CultureInfo.InvariantCulture) switch
            {
                0 => MonsterGender.Male,
                1 => MonsterGender.Female,
                2 => MonsterGender.Genderless,
                _ => MonsterGender.Genderless,
            };

            IsShiny = int.Parse(dict["isShiny"], CultureInfo.InvariantCulture) != 0;

            var abilityId = short.Parse(dict["Ability"], CultureInfo.InvariantCulture);
            if (monsterStaticData.Abilities.First.StaticData.Id == abilityId)
            {
                Ability = monsterStaticData.Abilities.First;
            }
            else if (monsterStaticData.Abilities.Second?.StaticData.Id == abilityId)
            {
                Ability = monsterStaticData.Abilities.Second;
            }
            else
            {
                Ability = null!;
            }

            Nature = byte.Parse(dict["Nature"], CultureInfo.InvariantCulture);

            Experience = int.Parse(dict["Experience"], CultureInfo.InvariantCulture);
            Friendship = byte.Parse(dict["Friendship"], CultureInfo.InvariantCulture);
            EggSteps = int.Parse(dict["EggSteps"], CultureInfo.InvariantCulture);
            CatchInfo = new CatchInfo
            {
                Nickname = string.IsNullOrEmpty(dict["NickName"]) ? null : dict["NickName"],
                ContainerId = byte.Parse(dict["CatchBall"], CultureInfo.InvariantCulture),
                Method = dict["CatchMethod"],
                Location = dict["CatchLocation"],
                TrainerName = dict["CatchTrainer"],
                TrainerId = dict.TryGetValue("OT", out var otStr) ? string.IsNullOrEmpty(otStr) ? null : uint.TryParse(otStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out var ot) ? ot : 0 : null,
            };

            //if (short.TryParse(dict["Item"], out var itemId) && itemId != 0)
            //    HeldItem = new Item(getItem(itemId));
            HeldItem = heldItem;

            Attacks = attacks;

            CurrentHP = short.Parse(dict["HP"], CultureInfo.InvariantCulture);

            var iv = dict["IVs"].Split(',');
            var iv0 = short.Parse(iv[0], CultureInfo.InvariantCulture);
            var iv1 = short.Parse(iv[1], CultureInfo.InvariantCulture);
            var iv2 = short.Parse(iv[2], CultureInfo.InvariantCulture);
            var iv3 = short.Parse(iv[3], CultureInfo.InvariantCulture);
            var iv4 = short.Parse(iv[4], CultureInfo.InvariantCulture);
            var iv5 = short.Parse(iv[5], CultureInfo.InvariantCulture);
            IV = new Stats(iv0, iv1, iv2, iv3, iv4, iv5);

            var ev = dict["EVs"].Split(',');
            var ev0 = short.Parse(ev[0], CultureInfo.InvariantCulture);
            var ev1 = short.Parse(ev[1], CultureInfo.InvariantCulture);
            var ev2 = short.Parse(ev[2], CultureInfo.InvariantCulture);
            var ev3 = short.Parse(ev[3], CultureInfo.InvariantCulture);
            var ev4 = short.Parse(ev[4], CultureInfo.InvariantCulture);
            var ev5 = short.Parse(ev[5], CultureInfo.InvariantCulture);
            EV = new Stats(ev0, ev1, ev2, ev3, ev4, ev5);

            Metadata.Add("OriginalNumber", dict["OriginalNumber"]);
            Metadata.Add("IDValue", dict["IDValue"]);
            Metadata.Add("ItemData", dict["ItemData"]);
            Metadata.Add("AdditionalData", dict["AdditionalData"]);
            Metadata.Add("Status", dict["Status"]);
        }

        public override string ToString() => !string.IsNullOrWhiteSpace(CatchInfo.Nickname) ? CatchInfo.Nickname : StaticData.Name;
    }
}