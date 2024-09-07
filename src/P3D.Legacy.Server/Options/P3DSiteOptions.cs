using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

using System.Net.Http;

namespace P3D.Legacy.Server.Options;

public sealed class P3DSiteOptionsValidator : AbstractValidator<P3DSiteOptions>
{
    public P3DSiteOptionsValidator(HttpClient httpClient)
    {
        RuleFor(static x => x.APIEndpointV1).IsUri().IsUriAvailable(httpClient).When(static x => !string.IsNullOrEmpty(x.APIEndpointV1));
        RuleFor(static x => x.APIToken).NotEmpty().When(static x => !string.IsNullOrEmpty(x.APIEndpointV1));
    }
}

public sealed record P3DSiteOptions
{
    public required string APIEndpointV1 { get; init; }
    public required string APIToken { get; init; }
}