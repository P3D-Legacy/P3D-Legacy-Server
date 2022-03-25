using FluentValidation;

using P3D.Legacy.Server.Abstractions.FluentValidation;

using System;

namespace P3D.Legacy.Server.Infrastructure.Extensions
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, string> IsLiteDBConnectionString<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            if (ruleBuilder == null)
            {
                throw new ArgumentNullException(nameof(ruleBuilder));
            }

            return ruleBuilder.SetValidator(new FluentValidation.IsLiteDBConnectionStringValidator<T>());
        }
    }
}