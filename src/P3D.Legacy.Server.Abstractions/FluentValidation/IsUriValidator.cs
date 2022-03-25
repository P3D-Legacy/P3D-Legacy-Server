using FluentValidation;
using FluentValidation.Validators;

using System;

namespace P3D.Legacy.Server.Abstractions.FluentValidation
{
    public interface IIsUriValidator : IPropertyValidator { }

    public class IsUriValidator<T> : PropertyValidator<T, string>, IIsUriValidator
    {
        public override string Name => "IsUriValidator";

        public override bool IsValid(ValidationContext<T> context, string value) => value switch
        {
            { } s when Uri.TryCreate(s, UriKind.Absolute, out _) => true,
            _ => false
        };

        protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} is not an uri!";
    }
}