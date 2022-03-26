using Aragas.Extensions.Options.FluentValidation.Extensions;

using FluentValidation;

using System.Net.Http;

namespace P3D.Legacy.Server.Options
{
    public sealed class P3DSiteOptionsValidator : AbstractValidator<P3DSiteOptions>
    {
        public P3DSiteOptionsValidator(HttpClient httpClient)
        {
            RuleFor(x => x.APIEndpointV1).IsUri().IsUriAvailable(httpClient);
            RuleFor(x => x.APIToken).NotEmpty().When(x => !string.IsNullOrEmpty(x.APIEndpointV1));
        }
    }

    public sealed record P3DSiteOptions
    {
        public string APIEndpointV1 { get; init; } = default!;
        public string APIToken { get; init; } = default!;
    }
}