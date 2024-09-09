using FluentValidation;

using System.Net.Http;

namespace P3D.Legacy.Server.Options;

public sealed class P3DSiteOptionsValidator : AbstractValidator<P3DSiteOptions>
{
    public P3DSiteOptionsValidator(HttpClient httpClient)
    {

    }
}

public sealed record P3DSiteOptions
{
    public required string APIToken { get; set; }
}