using FluentValidation;
using FluentValidation.Validators;

using System.Net;

namespace P3D.Legacy.Server.Abstractions.FluentValidation
{
    public interface IIsIPEndPointValidator : IPropertyValidator { }

    public class IsIPEndPointValidator<T> : PropertyValidator<T, string>, IIsIPEndPointValidator
    {
        public override string Name => "IsIPEndPointValidator";

        public override bool IsValid(ValidationContext<T> context, string value) => value switch
        {
            { } s when IPEndPoint.TryParse(s, out _) => true,
            _ => false
        };

        protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} is not an IP Endpoint!";
    }
}