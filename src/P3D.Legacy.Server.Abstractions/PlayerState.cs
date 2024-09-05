namespace P3D.Legacy.Server.Abstractions;

public enum PlayerState
{
    None,
    Initializing,
    Authentication,
    Initialized,
    Finalizing,
    Finalized,
}