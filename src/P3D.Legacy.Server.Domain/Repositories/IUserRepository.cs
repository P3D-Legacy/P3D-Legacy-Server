using P3D.Legacy.Common;
using P3D.Legacy.Server.Domain.Entities.Users;
using P3D.Legacy.Server.Domain.Models;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Domain.Repositories;

public interface IUserRepository
{
    Task<UserEntity?> FindByIdAsync(PlayerId playerId, CancellationToken ct);
    Task<AccountResult> CreateAsync(UserEntity user, string password, bool isSha512 = false, CancellationToken ct = default);

    Task<DateTimeOffset?> GetLockoutEndDateAsync(UserEntity user, CancellationToken ct);
    Task<int> GetAccessFailedCountAsync(UserEntity user, CancellationToken ct);

    Task<SignInResult> CheckPasswordSignInAsync(UserEntity user, string password, bool isSha512 = false, bool lockoutOnFailure = true, CancellationToken ct = default);
}