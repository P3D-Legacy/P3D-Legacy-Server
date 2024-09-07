namespace P3D.Legacy.Server.Domain;

public enum PlayerState
{
    None,
    Initializing,
    Authentication,
    Initialized,
    Finalizing,
    Finalized,
}