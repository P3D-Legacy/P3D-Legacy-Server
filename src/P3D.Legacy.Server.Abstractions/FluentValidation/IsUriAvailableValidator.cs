using FluentValidation;
using FluentValidation.Validators;

using System;
using System.Net.Http;
using System.Threading;

namespace P3D.Legacy.Server.Abstractions.FluentValidation
{
    public interface IIsUriAvailableValidator : IPropertyValidator { }

    public class IsUriAvailableValidator<T> : PropertyValidator<T, string>, IIsUriAvailableValidator
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public override string Name => "IsUriAvailableValidator";

        public IsUriAvailableValidator(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public override bool IsValid(ValidationContext<T> context, string value)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("FluentClient");
                var request = new HttpRequestMessage(HttpMethod.Options, value);
                var cts = new CancellationTokenSource(2000);
                var response = client.Send(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
                return true;
            }
            catch (Exception e)
            {
                context.MessageFormatter.AppendArgument("RequestException", e);
                return false;
            }
        }

        protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} has an invalid uri! Message:\n{RequestException}";
    }
}