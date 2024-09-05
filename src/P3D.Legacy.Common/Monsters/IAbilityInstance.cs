namespace P3D.Legacy.Common.Monsters;

public interface IAbilityInstance
{
    IAbilityStaticData StaticData { get; }

    bool IsHidden { get; }

    string? ToString() => $"{StaticData}{(IsHidden ? " (H)" : string.Empty)}";
}