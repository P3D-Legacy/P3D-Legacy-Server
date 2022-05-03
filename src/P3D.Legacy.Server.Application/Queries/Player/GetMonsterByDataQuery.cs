using P3D.Legacy.Common.Monsters;
using P3D.Legacy.Server.CQERS.Queries;

namespace P3D.Legacy.Server.Application.Queries.Player
{
    public sealed record GetMonsterByDataQuery(string MonsterData) : IQuery<IMonsterInstance>;
}