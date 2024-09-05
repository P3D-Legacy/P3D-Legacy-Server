namespace P3D.Legacy.Common.Monsters;

public interface IItemInstance
{
    public IItemStaticData StaticData { get; }

    string? ToString() => $"{StaticData}";
}