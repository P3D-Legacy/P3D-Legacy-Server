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
        private readonly HttpClient _httpClient;

        public override string Name => "IsUriAvailableValidator";

        public IsUriAvailableValidator(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public override bool IsValid(ValidationContext<T> context, string value)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Options, value);
                var cts = new CancellationTokenSource(2000);
                _ = _httpClient.Send(request, HttpCompletionOption.ResponseHeadersRead, cts.Token);
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