using System;

namespace GitGameServer
{
    [Flags]
    public enum GameSettings
    {
        // The numerical values below are intended as flags (1, 2, 4, 8, ...) and are used for persistency.
        ExcludeMerges = 1,
        LowerCase = 2
    }
}