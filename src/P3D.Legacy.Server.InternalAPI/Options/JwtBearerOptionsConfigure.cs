using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using System;

namespace P3D.Legacy.Server.InternalAPI.Options
{
    public sealed class JwtBearerOptionsConfigure : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly JwtOptions _jwtOptions;

        public JwtBearerOptionsConfigure(IOptions<JwtOptions> jwtOptions)
        {
            _jwtOptions = jwtOptions.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        }

        public void Configure(JwtBearerOptions options) => Configure(Microsoft.Extensions.Options.Options.DefaultName, options);
        public void Configure(string name, JwtBearerOptions options)
        {
            // Only configure the options if this is the correct instance
            if (string.Equals(name, JwtBearerDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = "P3D Legacy Server",
                    ValidateIssuer = true,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidateActor = false,
                    ValidateTokenReplay = false,
                    IssuerSigningKey = new RsaSecurityKey(_jwtOptions.GetKey()),
                    ClockSkew = TimeSpan.FromMinutes(5),
                };
            }
        }
    }
}