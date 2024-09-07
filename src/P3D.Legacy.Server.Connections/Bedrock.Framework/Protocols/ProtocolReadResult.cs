using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace
namespace Bedrock.Framework.Protocols;

[StructLayout(LayoutKind.Auto)]
public readonly record struct ProtocolReadResult<TMessage>(TMessage Message, bool IsCanceled, bool IsCompleted);