using P3D.Legacy.Common.Monsters;

namespace P3D.Legacy.Server.Infrastructure.Models.Monsters;

public sealed record ItemEntity(IItemStaticData StaticData) : IItemInstance;