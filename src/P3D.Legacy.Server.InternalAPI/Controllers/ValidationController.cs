using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using System;
using System.IdentityModel.Tokens.Jwt;

namespace P3D.Legacy.Server.InternalAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ValidationController : ControllerBase
{
    private const string Bearer = "Bearer ";

    private readonly JwtBearerOptions _jwtBearerOptions;

    public ValidationController(IOptionsSnapshot<JwtBearerOptions> jwtBearerOptions)
    {
        _jwtBearerOptions = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme) ?? throw new ArgumentNullException(nameof(jwtBearerOptions));
    }

    [HttpGet]
    public IActionResult ValidateToken()
    {
        string? authorization = Request.Headers.Authorization;

        // If no authorization header found, nothing to process further
        if (string.IsNullOrEmpty(authorization))
            return BadRequest(AuthenticateResult.NoResult());

        if (authorization.StartsWith(Bearer, StringComparison.OrdinalIgnoreCase))
        {
            var token = authorization.Substring(Bearer.Length).Trim();
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                tokenHandler.ValidateToken(token, _jwtBearerOptions.TokenValidationParameters, out _);

                return Ok();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }

        return BadRequest();
    }
}