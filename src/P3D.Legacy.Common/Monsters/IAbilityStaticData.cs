namespace P3D.Legacy.Common.Monsters
{
    public interface IAbilityStaticData
    {
        short Id { get; }
        string Name { get; }

        string? ToString() => $"{Name}";
    }
}