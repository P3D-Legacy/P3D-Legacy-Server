using Isopoh.Cryptography.Argon2;
using Isopoh.Cryptography.SecureArray;

using LiteDB;
using LiteDB.Async;

using Microsoft.Extensions.Options;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models;
using P3D.Legacy.Server.Infrastructure.Models.Users;
using P3D.Legacy.Server.Infrastructure.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Infrastructure.Services.Users
{
    public class LiteDbUserManager : IUserManager
    {
        private record Lockout(ObjectId Id, string UserId, DateTimeOffset LockoutEnd);
        private record User(string Id, string Name, string PasswordHash);

        private readonly LiteDbOptions _options;
        private readonly PasswordOptions _passwordOptions;
        private readonly LockoutOptions _lockoutOptions;

        public LiteDbUserManager(IOptions<LiteDbOptions> options, IOptions<PasswordOptions> passwordOptions, IOptions<LockoutOptions> lockoutOptions)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
            _passwordOptions = passwordOptions.Value ?? throw new ArgumentNullException(nameof(passwordOptions));
            _lockoutOptions = lockoutOptions.Value ?? throw new ArgumentNullException(nameof(lockoutOptions));
        }

        public async Task<UserEntity?> FindByIdAsync(PlayerId id, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<User>("users");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.Id, true);

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            return await collection.FindByIdAsync(idStr) is { } entry ? new UserEntity(PlayerId.Parse(entry.Id), entry.Name) : null;
        }

        public async Task<AccountResult> CreateAsync(UserEntity userEntity, string password, bool isSha512 = false, CancellationToken ct = default)
        {
            var (playerId, name) = userEntity;

            ct.ThrowIfCancellationRequested();

            if (isSha512) password += "A!";

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<User>("users");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.Id, true);

            var validationResult = await ValidateAsync(password, ct);

            ct.ThrowIfCancellationRequested();
            if (!isSha512) password = BitConverter.ToString(new SHA512Managed().ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "").ToLower() + "A!";
            var pwHash = GetPasswordHash(password);
            var user = new User(playerId.ToString(), name, pwHash);

            ct.ThrowIfCancellationRequested();
            var result = await collection.InsertAsync(user);

            return validationResult;
        }

        public async Task<DateTimeOffset?> GetLockoutEndDateAsync(UserEntity userEntity, CancellationToken ct)
        {
            var (id, _) = userEntity;

            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<Lockout>("lockouts");

            ct.ThrowIfCancellationRequested();
            await collection.DeleteManyAsync(x => x.LockoutEnd < DateTimeOffset.UtcNow);

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.Id, true);

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.UserId, false);

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            var minValue = await collection.Query().Where(x => x.UserId == idStr).OrderBy(x => x.LockoutEnd).FirstOrDefaultAsync();
            return minValue?.LockoutEnd;
        }

        public async Task<int> GetAccessFailedCountAsync(UserEntity userEntity, CancellationToken ct)
        {
            var (id, _) = userEntity;

            ct.ThrowIfCancellationRequested();

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<Lockout>("lockouts");

            ct.ThrowIfCancellationRequested();
            await collection.DeleteManyAsync(x => x.LockoutEnd < DateTimeOffset.UtcNow);

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.Id, true);

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.UserId, false);

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            var entryCount = await collection.CountAsync(Query.EQ(nameof(Lockout.UserId), idStr));

            return entryCount;
        }

        public async Task<SignInResult> CheckPasswordSignInAsync(UserEntity userEntity, string password, bool isSha512 = false, bool lockoutOnFailure = true, CancellationToken ct = default)
        {
            var (id, _) = userEntity;

            ct.ThrowIfCancellationRequested();

            if (isSha512) password += "A!";

            using var db = new LiteDatabaseAsync(_options.ConnectionString);
            var collection = db.GetCollection<User>("users");
            var locksCollection = db.GetCollection<Lockout>("lockouts");

            ct.ThrowIfCancellationRequested();
            await collection.EnsureIndexAsync(x => x.Id, true);

            if (await GetAccessFailedCountAsync(userEntity, ct) >= _lockoutOptions.MaxFailedAccessAttempts)
                return SignInResult.LockedOut;

            var idStr = id.ToString();
            ct.ThrowIfCancellationRequested();
            var entry = await collection.FindByIdAsync(idStr);

            ct.ThrowIfCancellationRequested();
            if (!isSha512) password = BitConverter.ToString(new SHA512Managed().ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "").ToLower() + "A!";
            var result = VerifyPassword(entry.PasswordHash, password);

            if (!result)
            {
                ct.ThrowIfCancellationRequested();
                await locksCollection.InsertAsync(new Lockout(ObjectId.NewObjectId(), idStr, DateTimeOffset.UtcNow + _lockoutOptions.DefaultLockoutTimeSpan));

                return SignInResult.Failed;
            }

            return SignInResult.Success;
        }


        private Task<AccountResult> ValidateAsync(string password, CancellationToken ct)
        {
            static bool IsDigit(char c) => c is >= '0' and <= '9';
            static bool IsLower(char c) => c is >= 'a' and <= 'z';
            static bool IsUpper(char c) => c is >= 'A' and <= 'Z';
            static bool IsLetterOrDigit(char c) => IsUpper(c) || IsLower(c) || IsDigit(c);

            var errors = new List<GenericError>();

            if (string.IsNullOrWhiteSpace(password) || password.Length < _passwordOptions.RequiredLength)
            {
                errors.Add(ErrorDescriber.PasswordTooShort(_passwordOptions.RequiredLength));
            }
            if (_passwordOptions.RequireNonAlphanumeric && password.All(IsLetterOrDigit))
            {
                errors.Add(ErrorDescriber.PasswordRequiresNonAlphanumeric());
            }
            if (_passwordOptions.RequireDigit && !password.Any(IsDigit))
            {
                errors.Add(ErrorDescriber.PasswordRequiresDigit());
            }
            if (_passwordOptions.RequireLowercase && !password.Any(IsLower))
            {
                errors.Add(ErrorDescriber.PasswordRequiresLower());
            }
            if (_passwordOptions.RequireUppercase && !password.Any(IsUpper))
            {
                errors.Add(ErrorDescriber.PasswordRequiresUpper());
            }
            if (_passwordOptions.RequiredUniqueChars >= 1 && password.Distinct().Count() < _passwordOptions.RequiredUniqueChars)
            {
                errors.Add(ErrorDescriber.PasswordRequiresUniqueChars(_passwordOptions.RequiredUniqueChars));
            }
            return Task.FromResult(errors.Count == 0 ? AccountResult.Success : AccountResult.Failed(errors.ToArray()));
        }

        private static string GetPasswordHash(string password)
        {
            var salt = new byte[16];
            using var randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(salt);
            var config = new Argon2Config
            {
                Type = Argon2Type.DataIndependentAddressing,
                Version = Argon2Version.Nineteen,
                TimeCost = 10,
                MemoryCost = 32768,
                Lanes = 5,
                Threads = Environment.ProcessorCount, // higher than "Lanes" doesn't help (or hurt)
                Password = Encoding.UTF8.GetBytes(password),
                Salt = salt, // >= 8 bytes if not null
                //Secret = secret, // from somewhere
                //AssociatedData = associatedData, // from somewhere
                HashLength = 20 // >= 4
            };
            using var argon2A = new Argon2(config);
            using var hashA = argon2A.Hash();
            return config.EncodeString(hashA.Buffer);
        }

        private static bool VerifyPassword(string hash, string password)
        {
            var configOfPasswordToVerify = new Argon2Config
            {
                Password = Encoding.UTF8.GetBytes(password),
                Threads = Environment.ProcessorCount
            };
            SecureArray<byte>? hashB = null;
            try
            {
                if (configOfPasswordToVerify.DecodeString(hash, out hashB) && hashB is not null)
                {
                    using var argon2ToVerify = new Argon2(configOfPasswordToVerify);
                    using var hashToVerify = argon2ToVerify.Hash();
                    return Argon2.FixedTimeEquals(hashB, hashToVerify);
                }
            }
            finally
            {
                hashB?.Dispose();
            }

            return false;
        }
    }
}