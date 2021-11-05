using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models;
using P3D.Legacy.Server.Infrastructure.Models.Users;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Services.Users
{

    public interface IUserManager
    {
        Task<UserEntity?> FindByIdAsync(PlayerId playerId, CancellationToken ct);
        Task<AccountResult> CreateAsync(UserEntity user, string password, bool isSha512 = true, CancellationToken ct = default);

        Task<DateTimeOffset?> GetLockoutEndDateAsync(UserEntity user, CancellationToken ct);
        Task<int> GetAccessFailedCountAsync(UserEntity user, CancellationToken ct);

        Task<SignInResult> CheckPasswordSignInAsync(UserEntity user, string password, bool isSha512 = true, bool lockoutOnFailure = true, CancellationToken ct = default);
    }
}