using P3D.Legacy.Common.Monsters;

namespace P3D.Legacy.Server.Domain.Entities.Monsters;

public sealed record AttackEntity(IAttackStaticData StaticData, byte PPCurrent, byte PPUps) : IAttackInstance;