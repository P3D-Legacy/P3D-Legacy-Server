using FluentValidation;

using P3D.Legacy.Server.FluentValidation;

using System;
using System.Net.Http;

namespace P3D.Legacy.Server.Extensions
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, string> IsGrpcAvailable<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            if (ruleBuilder == null)
            {
                throw new ArgumentNullException(nameof(ruleBuilder));
            }

            return ruleBuilder.SetValidator(new IsGrpcAvailableValidator<T>());
        }
    }
}