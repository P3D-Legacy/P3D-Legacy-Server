using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using P3D.Legacy.Common;
using P3D.Legacy.Server.Infrastructure.Models.Users;
using P3D.Legacy.Server.Infrastructure.Repositories.Users;
using P3D.Legacy.Server.InternalAPI.Options;
using P3D.Legacy.Server.UI.Shared.Models;

using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.InternalAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LoginController : ControllerBase
{
    private readonly JwtOptions _jwtOptions;
    private readonly IUserRepository _userRepository;

    public LoginController(IOptions<JwtOptions> jwtOptions, IUserRepository userRepository)
    {
            _jwtOptions = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

    [HttpPost]
    public async Task<IActionResult> LoginAsync([FromBody] LoginModel login, CancellationToken ct)
    {
            var user = await _userRepository.FindByIdAsync(PlayerId.FromName(login.Username), ct);

            if (user is null) return BadRequest(new LoginResult { Successful = false, Error = "User does not exists." });

            var result = await _userRepository.CheckPasswordSignInAsync(user, login.Password, isSha512: false, lockoutOnFailure: true, ct);
            if (!result.Succeeded)
            {
                return BadRequest(new LoginResult { Successful = false, Error = "Username and password are invalid." });
            }

            return Ok(new LoginResult { Successful = true, Token = GenerateJsonWebToken(user) });
        }

    private string GenerateJsonWebToken(UserEntity user)
    {
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = GetSecurityToken(user);
            return tokenHandler.WriteToken(securityToken);
        }
    private JwtSecurityToken GetSecurityToken(UserEntity user)
    {
            var signingCredentials = _jwtOptions.KeyType switch
            {
                KeyType.Rsa => new SigningCredentials(new RsaSecurityKey(_jwtOptions.GetRSAKey()), SecurityAlgorithms.RsaSha512Signature),
                KeyType.ECDsa => new SigningCredentials(new ECDsaSecurityKey(_jwtOptions.GetECDsaKey()), SecurityAlgorithms.EcdsaSha512Signature),
                _ => throw new NotSupportedException()
            };

            var now = DateTime.UtcNow;
            var unixTimeSeconds = new DateTimeOffset(now).ToUnixTimeSeconds();

            const int expiringDays = 7;

            return new JwtSecurityToken(
                issuer: "P3D Legacy Server",
                claims: new Claim[]
                {
                    new(JwtRegisteredClaimNames.Iat, unixTimeSeconds.ToString(CultureInfo.InvariantCulture), ClaimValueTypes.Integer64),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(nameof(user.Id).ToLowerInvariant(), user.Id.ToString()),
                    new(nameof(user.Name).ToLowerInvariant(), user.Name),
                },
                notBefore: now,
                expires: now.AddDays(expiringDays),
                signingCredentials: signingCredentials
            );
        }
}