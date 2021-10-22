namespace P3D.Legacy.Common.Monsters
{
    public interface IAttackStaticData
    {
        ushort Id { get; }
        string Name { get; }

        byte PP { get; }

        string? ToString() => $"[Name: {Name,-20} PP: {PP,2}]";
    }
}