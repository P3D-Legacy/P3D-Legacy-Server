using System;

namespace P3D.Legacy.Common.Monsters
{
    public interface IAttackInstance
    {
        IAttackStaticData StaticData { get; }

        byte PPCurrent { get; }
        byte PPUps { get; }
        byte PPMax => (byte) (StaticData.PP + (byte) Math.Round((double) StaticData.PP * PPUps * 0.20D));

        string? ToString() => $"{PPCurrent,2}/{PPMax,2} {StaticData}";
    }
}