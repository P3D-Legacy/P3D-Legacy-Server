using P3D.Legacy.Common.Monsters;

namespace P3D.Legacy.Server.Domain.Entities.Monsters;

public sealed record AttackStaticDataEntity(ushort Id, string Name, byte PP) : IAttackStaticData;