using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using P3D.Legacy.Common.Monsters;
using P3D.Legacy.Server.Domain.Options;
using P3D.Legacy.Server.Domain.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.Application.Services;

public class DefaultMonsterValidator : IMonsterValidator
{
    private readonly ILogger _logger;
    private readonly PokeAPIOptions _options;

    public DefaultMonsterValidator(ILogger<DefaultMonsterValidator> logger, IOptions<PokeAPIOptions> options)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
    public Task<bool> ValidateAsync(IMonsterInstance monster, CancellationToken ct) => Task.FromResult(
        (monster.Gender == MonsterGender.Male && monster.StaticData.MaleRatio == 0 ||
         monster.Gender == MonsterGender.Female && Math.Abs(monster.StaticData.MaleRatio - 1) < float.Epsilon ||
         monster.Gender == MonsterGender.Genderless && Math.Abs(monster.StaticData.MaleRatio - (-1)) < float.Epsilon ||
         monster.Gender is MonsterGender.Male or MonsterGender.Female)
        && monster.Ability is not null
        //&& Attacks.All(a => StaticData.LearnableAttacks.Any(la => la.Id == a.StaticData.Id))
        //&& CurrentHP <= EV.HP + IV.HP
        && monster.Level <= 100
        && monster.EV.IsValidEV());
}