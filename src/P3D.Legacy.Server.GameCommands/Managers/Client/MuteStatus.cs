using System;

namespace P3D.Legacy.Server.GameCommands.Managers.Client
{
    [Flags]
    public enum MuteStatus
    {
        None                = 0,
        Completed           = 1,
        ClientNotFound      = 2,
        MutedYourself       = 4,
        IsNotMuted          = 8,
    }
}