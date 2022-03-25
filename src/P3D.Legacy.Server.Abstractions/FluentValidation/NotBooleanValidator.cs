using FluentValidation;
using FluentValidation.Validators;

namespace P3D.Legacy.Server.Abstractions.FluentValidation
{
    public interface INotBooleanValidator : IPropertyValidator { }

    public class NotBooleanValidator<T> : PropertyValidator<T, string>, INotBooleanValidator
    {
        public override string Name => "NotBooleanValidator";

        public override bool IsValid(ValidationContext<T> context, string value) => value switch
        {
            { } s when !bool.TryParse(s, out _) => true,
            _ => false
        };

        protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} is an boolean!";
    }
}