using FluentValidation;

using P3D.Legacy.Server.Abstractions.Extensions;

using System.Net.Http;

namespace P3D.Legacy.Server.Options
{
    public sealed class OtlpOptionsValidator : AbstractValidator<OtlpOptions>
    {
        public OtlpOptionsValidator(IHttpClientFactory httpClientFactory)
        {
            RuleFor(x => x.Host).IsUri().IsUriAvailable(httpClientFactory).When(x => x.Enabled);
        }
    }

    public sealed record OtlpOptions
    {
        public bool Enabled { get; init; } = default!;
        public string Host { get; init; } = default!;
    }
}