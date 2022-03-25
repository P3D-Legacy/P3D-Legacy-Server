using FluentValidation;
using FluentValidation.Validators;

using Grpc.Core;
using Grpc.Net.Client;

using System;
using System.Net;
using System.Net.Http;
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
                if (!IPEndPoint.TryParse(value, out var endPoint))
                {
                    return false;
                }

                using var client = new TcpClient();
                client.Connect(endPoint);
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