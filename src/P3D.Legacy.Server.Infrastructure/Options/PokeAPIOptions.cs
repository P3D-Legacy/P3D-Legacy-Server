using FluentValidation;

using P3D.Legacy.Server.Abstractions.Extensions;

using System.Net.Http;

namespace P3D.Legacy.Server.Infrastructure.Options
{
    public sealed class PokeAPIOptionsValidator : AbstractValidator<PokeAPIOptions>
    {
        public PokeAPIOptionsValidator(IHttpClientFactory httpClientFactory)
        {
            RuleFor(x => x.GraphQLEndpoint).IsUri().IsUriAvailable(httpClientFactory).When(x => !string.IsNullOrEmpty(x.GraphQLEndpoint));
        }
    }

    public record PokeAPIOptions
    {
        public string GraphQLEndpoint { get; init; } = default!;
    }
}