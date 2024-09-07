using P3D.Legacy.Common.Monsters;

namespace P3D.Legacy.Server.Domain.Entities.Monsters;

public sealed record AbilityStaticDataEntity(short Id, string Name) : IAbilityStaticData;