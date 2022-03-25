using FluentValidation;
using FluentValidation.Validators;

using System;
using System.Net;
using System.Net.Sockets;

namespace P3D.Legacy.Server.FluentValidation
{
    public interface IIsGrpcAvailableValidator : IPropertyValidator { }

    public class IsGrpcAvailableValidator<T> : PropertyValidator<T, string>, IIsGrpcAvailableValidator
    {
        public override string Name => "IsGrpcAvailableValidator";


        public override bool IsValid(ValidationContext<T> context, string value)
        {
            try
            {
                var uri = new Uri(value);
                var host = uri.GetComponents(UriComponents.Host, UriFormat.Unescaped);
                var portStr = uri.GetComponents(UriComponents.Port, UriFormat.Unescaped);
                if (!ushort.TryParse(portStr, out var port))
                {
                    return false;
                }

                using var client = new TcpClient();
                client.Connect(host, port);
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