using P3D.Legacy.Common;

namespace P3D.Legacy.Server.Application.Queries.Players
{
    public record PlayerViewModel(long Id, string Name, GameJoltId GameJoltId);
}