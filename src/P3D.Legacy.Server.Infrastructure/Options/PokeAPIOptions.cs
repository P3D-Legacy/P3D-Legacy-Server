using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

using System.Net.Http;

namespace P3D.Legacy.Server.Infrastructure.Options;

public sealed class PokeAPIOptionsValidator : AbstractValidator<PokeAPIOptions>
{
    public PokeAPIOptionsValidator(HttpClient httpClient)
    {
        RuleFor(static x => x.GraphQLEndpoint).IsUri().IsUriAvailable(httpClient).When(static x => !string.IsNullOrEmpty(x.GraphQLEndpoint));
    }
}

public sealed record PokeAPIOptions
{
    public required string GraphQLEndpoint { get; init; } = default!;
}