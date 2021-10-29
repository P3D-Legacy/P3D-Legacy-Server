using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using P3D.Legacy.Server.Abstractions.Identity;
using P3D.Legacy.Server.InternalAPI.Options;
using P3D.Legacy.Server.UI.Shared.Models;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace P3D.Legacy.Server.InternalAPI.Controllers
{
    public class GameJoltAuthenticationHandler : IAuthenticationHandler
    {
        /// <summary>
        /// Gets or sets the <see cref="AuthenticationScheme"/> asssociated with this authentication handler.
        /// </summary>
        public AuthenticationScheme Scheme { get; private set; } = default!;

        public Task InitializeAsync(AuthenticationScheme scheme, HttpContext context)
        {
            if (scheme is null)
            {
                throw new ArgumentNullException(nameof(scheme));
            }
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            Scheme = scheme;
            return Task.CompletedTask;
        }

        public Task<AuthenticateResult> AuthenticateAsync()
        {
            throw new NotImplementedException();
        }

        public Task ChallengeAsync(AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }

        public Task ForbidAsync(AuthenticationProperties? properties)
        {
            throw new NotImplementedException();
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly JwtOptions _jwtOptions;
        private readonly UserManager<ServerIdentity> _userManager;
        private readonly SignInManager<ServerIdentity> _signInManager;

        public LoginController(IOptions<JwtOptions> jwtOptions, UserManager<ServerIdentity> userManager, SignInManager<ServerIdentity> signInManager)
        {
            _jwtOptions = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var result = await _signInManager.PasswordSignInAsync(login.Email, login.Password, false, false);

            if (!result.Succeeded) return BadRequest(new LoginResult { Successful = false, Error = "Username and password are invalid." });

            return Ok(new LoginResult { Successful = true, Token = GenerateJsonWebToken(login) });
        }

        private string GenerateJsonWebToken(LoginModel loginModel)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = GetTokenDescriptor(loginModel);
            return tokenHandler.CreateEncodedJwt(tokenDescriptor);
        }
        private SecurityTokenDescriptor GetTokenDescriptor(LoginModel loginModel)
        {
            const int expiringDays = 7;
            var signingCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SignKey)), SecurityAlgorithms.HmacSha256Signature);
            var encryptingCredentials = new EncryptingCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.EncryptionKey)), SecurityAlgorithms.Aes128KW, SecurityAlgorithms.Aes128CbcHmacSha256);

            return new SecurityTokenDescriptor
            {
                Claims = new Dictionary<string, object>
                {
                    { ClaimTypes.Name, loginModel.Email },
                    { ClaimTypes.NameIdentifier, loginModel.Email },
                    { JwtRegisteredClaimNames.Sub, loginModel.Email },
                    { JwtRegisteredClaimNames.Email, loginModel.Email },
                },
                SigningCredentials = signingCredentials,
                EncryptingCredentials = encryptingCredentials,
                Expires = DateTime.UtcNow.AddDays(expiringDays),
            };
        }
    }
}