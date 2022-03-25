using FluentValidation;
using FluentValidation.Validators;

using LiteDB;

using System;
using System.IO;
using System.Linq;

namespace P3D.Legacy.Server.Infrastructure.FluentValidation
{
    public interface IIsLiteDBConnectionStringValidator : IPropertyValidator { }

    public class IsLiteDBConnectionStringValidator<T> : PropertyValidator<T, string>, IIsLiteDBConnectionStringValidator
    {
        public override string Name => "IsLiteDBConnectionStringValidator";

        public override bool IsValid(ValidationContext<T> context, string value)
        {
            try
            {
                var cs = new ConnectionString(value);
                var invalidChars = Path.GetInvalidFileNameChars().ToHashSet();
                return cs.Filename.Any(x => invalidChars.Contains(x));
            }
            catch (Exception)
            {
                return false;
            }
        }

        protected override string GetDefaultMessageTemplate(string errorCode) => "{PropertyName} is not a valid LiteDB Connection String!";
    }
}