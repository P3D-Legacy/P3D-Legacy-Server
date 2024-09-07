using P3D.Legacy.Common;

using System.Collections.Generic;

namespace P3D.Legacy.Server.Domain.Services;

public interface IPlayerContainerReader
{
    IPlayer? Get(Origin origin);
    IEnumerable<IPlayer> GetAll();
}