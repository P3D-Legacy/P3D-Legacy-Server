using Microsoft.AspNetCore.Connections;

// ReSharper disable once CheckNamespace
namespace Bedrock.Framework.Protocols;

public static class Protocol
{
    public static ProtocolWriter CreateWriter(this ConnectionContext connection) => new(connection.Transport.Output);

    public static ProtocolReader CreateReader(this ConnectionContext connection) => new(connection.Transport.Input);
}