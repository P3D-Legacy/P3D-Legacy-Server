using FluentValidation;

using P3D.Legacy.Server.Abstractions.FluentValidation;

using System;
using System.Net.Http;

namespace P3D.Legacy.Server.Abstractions.Extensions
{
    public static class RuleBuilderExtensions
    {
        public static IRuleBuilderOptions<T, string> NotInteger<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            if (ruleBuilder == null)
            {
                throw new ArgumentNullException(nameof(ruleBuilder));
            }

            return ruleBuilder.SetValidator(new NotIntegerValidator<T>());
        }

        public static IRuleBuilderOptions<T, string> NotBoolean<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            if (ruleBuilder == null)
            {
                throw new ArgumentNullException(nameof(ruleBuilder));
            }

            return ruleBuilder.SetValidator(new NotBooleanValidator<T>());
        }

        public static IRuleBuilderOptions<T, string> IsUri<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            if (ruleBuilder == null)
            {
                throw new ArgumentNullException(nameof(ruleBuilder));
            }

            return ruleBuilder.SetValidator(new IsUriValidator<T>());
        }

        public static IRuleBuilderOptions<T, string> IsUriAvailable<T>(this IRuleBuilder<T, string> ruleBuilder, IHttpClientFactory httpClientFactory)
        {
            if (ruleBuilder == null)
            {
                throw new ArgumentNullException(nameof(ruleBuilder));
            }

            return ruleBuilder.SetValidator(new IsUriAvailableValidator<T>(httpClientFactory));
        }

        public static IRuleBuilderOptions<T, string> IsIPAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
        {
            if (ruleBuilder == null)
            {
                throw new ArgumentNullException(nameof(ruleBuilder));
            }

            return ruleBuilder.SetValidator(new IsIPAddressValidator<T>());
        }
    }
}