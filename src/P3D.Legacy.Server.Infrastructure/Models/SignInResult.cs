namespace P3D.Legacy.Server.Infrastructure.Models;

public class SignInResult
{
    public static SignInResult Success { get; } = new() { Succeeded = true };
    public static SignInResult Failed { get; } = new();
    public static SignInResult LockedOut { get; } = new() { IsLockedOut = true };
    public static SignInResult NotAllowed { get; } = new() { IsNotAllowed = true };

    public bool Succeeded { get; protected set; }
    public bool IsLockedOut { get; protected set; }
    public bool IsNotAllowed { get; protected set; }

    public override string ToString() =>
        IsLockedOut ? "Lockedout" :
        IsNotAllowed ? "NotAllowed" :
        Succeeded ? "Succeeded" : "Failed";
}