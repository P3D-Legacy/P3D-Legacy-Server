using FluentValidation;
using FluentValidation.Validators;

namespace P3D.Legacy.Server.Abstractions.FluentValidation
{
    public interface INotIntegerValidator : IPropertyValidator { }

    public class NotIntegerValidator<T> : PropertyValidator<T, string>, INotIntegerValidator
    {
        public override string Name => "NotIntegerValidator";

        public override bool IsValid(ValidationContext<T> context, string value) => value switch
        {
            { } s when !long.TryParse(s, out _) && !decimal.TryParse(s, out _) => true,
            _ => false
        };

        protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} is an integer!";
    }
}