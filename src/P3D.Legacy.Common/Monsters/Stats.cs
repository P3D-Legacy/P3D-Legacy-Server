using System;

namespace P3D.Legacy.Common.Monsters
{
    public sealed record Stats(short HP, short Attack, short Defense, short SpecialAttack, short SpecialDefense, short Speed)
    {
        public static Stats None => new(0, 0, 0, 0, 0, 0);

        public short this[StatType statType] => statType switch
        {
            StatType.HP => HP,
            StatType.Attack => Attack,
            StatType.Defense => Defense,
            StatType.SpecialAttack => SpecialAttack,
            StatType.SpecialDefense => SpecialDefense,
            StatType.Speed => Speed,
            _ => throw new ArgumentOutOfRangeException()
        };

        public override string ToString() =>
            $"HP: {HP}, Atk: {Attack}, Def: {Defense}, SpAtt: {SpecialAttack}, SpDef: {SpecialDefense}, Spe: {Speed}";

        public bool IsValidIV() =>
            HP is >= 0 and <= 31 &&
            Attack is >= 0 and <= 31 &&
            Defense is >= 0 and <= 31 &&
            SpecialAttack is >= 0 and <= 31 &&
            SpecialDefense is >= 0 and <= 31 &&
            Speed is >= 0 and <= 31;

        // TODO: 255 or 252?
        public bool IsValidEV() =>
            HP is >= 0 and <= 255 &&
            Attack is >= 0 and <= 255 &&
            Defense is >= 0 and <= 255 &&
            SpecialAttack is >= 0 and <= 255 &&
            SpecialDefense is >= 0 and <= 255 &&
            Speed is >= 0 and <= 255 &&
            HP + Attack + Defense + SpecialAttack + SpecialDefense + Speed <= 510;
    }
}