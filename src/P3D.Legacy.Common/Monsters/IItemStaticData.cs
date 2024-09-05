namespace P3D.Legacy.Common.Monsters;

public interface IItemStaticData
{
    int Id { get; }
    string Name { get; }

    string? ToString() => $"{Name}";
}