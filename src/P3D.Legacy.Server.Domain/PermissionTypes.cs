using System;

namespace P3D.Legacy.Server.Domain;

[Flags]
public enum PermissionTypes
{
    None = 0,
    UnVerified = 1,
    User = 2,
    Debug = 4,
    Moderator = 8,
    Administrator = 16,
    Server = 32,


    UnVerifiedOrHigher = UnVerified | User | Moderator | Administrator | Server,
    UserOrHigher = User | Moderator | Administrator | Server,
    ModeratorOrHigher = Moderator | Administrator | Server,
    AdministratorOrHigher = Administrator | Server,
}